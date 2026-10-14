"""Validate NuGet JSON v1; reject incomplete scans and high/critical advisories."""
import json
from pathlib import Path
import sys
import xml.etree.ElementTree as ET


def check(report, expected):
    if report.get("version") != 1 or not report.get("sources"):
        raise ValueError("Missing scan sources or unsupported report version")
    if report.get("problems"):
        raise ValueError("NuGet reported scan problems")
    projects = report["projects"]
    paths = [Path(project["path"]).resolve() for project in projects]
    if len(paths) != len(expected) or set(paths) != expected:
        raise ValueError("Scan did not cover every solution project")
    blocked = False
    for project in projects:
        if project.get("problems"):
            raise ValueError("NuGet reported project scan problems")
        for framework in project.get("frameworks", []):
            for group in ("topLevelPackages", "transitivePackages"):
                for package in framework.get(group, []):
                    vulnerabilities = package["vulnerabilities"]
                    if not isinstance(vulnerabilities, list) or not vulnerabilities:
                        raise ValueError("Missing vulnerability metadata")
                    for vulnerability in vulnerabilities:
                        severity = vulnerability["severity"].lower()
                        if severity not in {"low", "moderate", "high", "critical"}:
                            raise ValueError("Unknown vulnerability severity")
                        if not vulnerability.get("advisoryurl"):
                            raise ValueError("Missing advisory URL")
                        print(f"{package['id']}: {severity} {vulnerability['advisoryurl']}")
                        blocked |= severity in {"high", "critical"}
    return blocked


def self_test():
    from copy import deepcopy

    expected = {Path("test.csproj").resolve()}
    clean = {"version": 1, "sources": ["https://api.nuget.org/v3/index.json"],
             "projects": [{"path": "test.csproj"}]}
    assert not check(clean, expected)
    for group in ("topLevelPackages", "transitivePackages"):
        for severity in ("Low", "Moderate", "High", "Critical"):
            report = deepcopy(clean)
            report["projects"][0]["frameworks"] = [{group: [{
                "id": "Test.Package", "vulnerabilities": [{
                    "severity": severity, "advisoryurl": "https://example.com/advisory"}]}]}]
            assert check(report, expected) == (severity in {"High", "Critical"})
    for bad in ({}, dict(clean, projects=[]), dict(clean, problems=[{"text": "offline"}]),
                dict(clean, sources=[]), dict(clean, version=2)):
        try:
            check(bad, expected)
        except (ValueError, KeyError, TypeError, AttributeError):
            continue
        raise AssertionError("Invalid scan accepted")
    print("Vulnerability gate self-test passed")


if __name__ == "__main__":
    if sys.argv[1:] == ["--self-test"]:
        self_test()
    else:
        try:
            solution = Path("src/CivicConnect.slnx")
            expected = {(solution.parent / project.attrib["Path"]).resolve()
                        for project in ET.parse(solution).iter("Project")}
            with open(sys.argv[1], encoding="utf-8-sig") as file:
                blocked = check(json.load(file), expected)
            sys.exit(1 if blocked else 0)
        except (ValueError, KeyError, TypeError, AttributeError, OSError, IndexError) as error:
            sys.exit(f"Vulnerability scan invalid: {error}")
