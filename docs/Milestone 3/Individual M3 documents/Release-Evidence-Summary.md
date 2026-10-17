# Release Evidence Summary (M3-REL-01, #98) — P3 draft

One concise table per the issue: result, interpretation, concern/action per area.
Each row links to CI runs or repo reports. "CI run" links below mean the branch
CI for the covering PR; re-link exact run URLs at tag time.

| Area | Result | Interpretation | Concern / action |
|---|---|---|---|
| Build | `dotnet build slnx` 0 warnings, 0 errors (analyzer gate incl.) | Solution compiles under warnings-as-errors | None |
| CI | Workflow green on phase PRs (build, migrations on disposable PG, format, vuln, coverage, tests, artifacts) | Pipeline enforces the gates | PR under review must show green CI before merge |
| Regression | DEF-006 throwing-observer test; Closed→Open rejection; illegal-transition suite | Fixed defects stay fixed in-suite | Defect Register rows: P1's register (M3-DEF-01) |
| API / integration | `RequestControllerTests` green (auth, RBAC, transitions, comments, assign, staff queue) | Endpoints enforce roles, ownership, policy | Production JWT validation not exercised (test-header scheme in `Testing`) |
| E2E / system | TC-S-01 (`E2EJourneyTests`, in suite) + TC-S-02 green; `scripts/staging-e2e.ps1` STAGING-E2E PASS tied to `/api/version` commit | Critical journeys work locally and on staging | FR-005 history asserted at DB level (no history endpoint... see note) |
| Static / security | Analyzer gate clean; `dotnet list --vulnerable` clean (see `docs/CI/M3-CI-07-findings.md` FIND-001..006 with dispositions) | No open tool findings; two mitigated `.editorconfig` exceptions remain | Advisories change over time; CI rescans every run (RR-SEC-02) |
| Performance | TC-P-01: 109,525 reqs, 0 errors, p95 23.71ms vs 3s target — PASS (`docs/P3-Decisions.md` #61/#63) | Headroom of ~100× on localhost scratch | Localhost-only claim; no production/load-mix evidence |
| Defects | DEF-006 verified fixed; pre-existing harness/authz issues fixed on main | No known open P3-area defects | One-table audit question + dead migration file: team calls, see debt register TD-04/TD-09 |
| Acceptance | RTM rows FR-014/015/016, NFR-020/024 verified test-host (+ live where stated) in `docs/RTM-M3.md` | Acceptable per implemented contract; FR-015/016 AC authored by P3 (no baseline wording existed) | RTM completion needs P1's remaining rows; readiness call is #99 |

Note (corrected 2026-10-15): FR-005 history endpoint **does** exist on current `main`
(`GET /api/requests/{id}/history`, P1 slice) — the TC-S-01 DB-level history assertion
predates it and stays valid, but a follow-up may assert via the endpoint instead.
