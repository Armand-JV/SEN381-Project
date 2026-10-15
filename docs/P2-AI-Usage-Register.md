# P2 AI Usage Register (M3)

Owner: P2. Format follows `docs/P3-AI-Usage-Register.md`. Human verification is **pending P2 review**. This draft records AI assistance but makes no claim that a person performed checks in this session.

| Date | Artefact | AI use | Human verification |
|---|---|---|---|
| 2026-10-15 | `scripts/coverage-summary.py` | AI wrote the script from issue #69 acceptance criteria: report Cobertura totals, no threshold, separate-report caveat, safe parsing, malformed-input failure | **Pending P2:** run `python3 scripts/coverage-summary.py --self-test`; review output and CI invocation |
| 2026-10-15 | `.github/workflows/test.yml` additions for #69 | AI drafted workflow steps: self-test before tests, collected Cobertura per CI spec, job summary after tests, artifact upload includes coverage XML | **Pending P2:** review workflow against #69; confirm no secrets, permissions, or artifact regressions |
| 2026-10-15 | `docs/RTM-M3.md` staff/security rows and issue #69 cross-reference | AI drafted requirement mappings, implementation paths, and issue linkages from current repository evidence | **Pending P2:** review every requirement mapping, source/test citation, status, and evidence limit against baseline PED and current code |
| 2026-10-15 | `docs/Milestone 3/PED-Section-5-CI-and-Coverage.md` | AI drafted coverage limitation note, critical-path gap list, and M3-CI-07 findings references from current repository evidence | **Pending P2:** verify each gap remains a requirement-specific claim, not a coverage metric; verify findings/register links; add hosted CI link only after run completes |
| 2026-10-15 | `docs/CI/M3-CI-07-findings.md`, `docs/Technical-Debt-Register.md`, `docs/Residual-Risk-Statement.md`, and `docs/Milestone 3/PED-Section-9-Defects-and-Quality.md` | AI normalized finding dispositions, added stable cross-links, and drafted debt/risk/PED §9 records from existing repository evidence | **Pending P2:** verify every tool, severity, decision, owner, scan-date claim, defect link, and accepted-risk/debt mapping; confirm P3 register ownership and repository-owner actions before merge |

Rejected/rewritten AI output this session: an initial RTM header would have claimed unverified PED and hosted-CI evidence. It was removed; pending evidence remains explicit. No hosted setting, credential rotation, history rewrite, or human review is claimed by the M3-CI-07 documentation.

Do not mark this register verified until P2 completes the checks above and replaces the applicable pending cells with dated results.
