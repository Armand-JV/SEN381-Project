#!/usr/bin/env python3
"""Print Coverlet's Cobertura totals without imposing a coverage threshold."""

import argparse
import os
from pathlib import Path
import sys
import tempfile
from xml.etree import ElementTree as ET


def metric(root, name):
    covered = int(root.attrib[f"{name}-covered"])
    valid = int(root.attrib[f"{name}-valid"])
    if not 0 <= covered <= valid:
        raise ValueError(f"invalid {name} counts: {covered}/{valid}")
    rate = f"{100 * covered / valid:.2f}%" if valid else "N/A"
    return f"{covered}/{valid} ({rate})"


def build_summary(directory):
    reports = sorted(directory.rglob("coverage.cobertura.xml"))
    if not reports:
        raise ValueError(f"no Cobertura reports found in {directory}")
    rows = ["## Cobertura coverage", "", "| Report | Lines | Branches |",
            "|---|---:|---:|"]
    for path in reports:
        root = ET.parse(path).getroot()
        if root.tag != "coverage":
            raise ValueError(f"{path} is not a Cobertura report")
        # Root totals avoid counting both method-level and class-level line entries.
        name = path.relative_to(directory).as_posix().replace("|", "&#124;").replace("`", "&#96;")
        rows.append(f"| `{name}` | {metric(root, 'lines')} | {metric(root, 'branches')} |")
    rows.extend(["", "Reports are separate; overlapping instrumentation is not summed.",
                 "Executed lines are not verified behaviour. No coverage threshold is enforced.",
                 "Download raw XML and TRX from the `test-results` artifact."])
    return "\n".join(rows) + "\n"


def self_test():
    with tempfile.TemporaryDirectory() as temporary:
        directory = Path(temporary)
        path = directory / "coverage.cobertura.xml"
        valid = '<coverage lines-covered="1" lines-valid="2" branches-covered="1" branches-valid="4"/>'
        for bad in (None, "<coverage", "<other/>", "<coverage/>",
                    valid.replace('lines-covered="1"', 'lines-covered="3"'),
                    valid.replace('lines-valid="2"', 'lines-valid="text"')):
            if bad is not None:
                path.write_text(bad, encoding="utf-8")
            try:
                build_summary(directory)
            except (ValueError, KeyError, ET.ParseError):
                pass
            else:
                raise AssertionError(f"invalid or missing report accepted: {bad}")
        path.write_text(valid, encoding="utf-8")
        summary = build_summary(directory)
        assert "1/2 (50.00%)" in summary and "1/4 (25.00%)" in summary
        second = directory / "second"
        second.mkdir()
        (second / path.name).write_text(
            '<coverage lines-covered="0" lines-valid="0" branches-covered="0" branches-valid="0"/>',
            encoding="utf-8")
        summary = build_summary(directory)
        assert summary.count("| `") == 2 and summary.count("0/0 (N/A)") == 2
        path.write_text(valid.replace('branches-covered="1"', 'branches-covered="-1"'), encoding="utf-8")
        try:
            build_summary(directory)
        except ValueError:
            pass
        else:
            raise AssertionError("negative counts accepted")
    print("coverage-summary self-test passed")


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--self-test", action="store_true")
    parser.add_argument("--results-directory", type=Path, default=Path("TestResults"))
    args = parser.parse_args()
    if args.self_test:
        self_test()
        return 0
    try:
        summary = build_summary(args.results_directory)
        if summary_path := os.environ.get("GITHUB_STEP_SUMMARY"):
            with open(summary_path, "a", encoding="utf-8") as output:
                output.write(summary)
        print(summary, end="")
    except (OSError, ET.ParseError, ValueError, KeyError) as error:
        print(f"coverage summary failed: {error}", file=sys.stderr)
        return 1
    return 0


if __name__ == "__main__":
    sys.exit(main())
