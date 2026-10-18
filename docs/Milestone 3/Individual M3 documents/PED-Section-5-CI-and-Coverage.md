# PED v3 §5 — CI and automated verification

## Coverage collection and interpretation

CI collects Coverlet Cobertura output during `dotnet test` and uploads the raw Cobertura XML and TRX files in the `test-results` artifact. `scripts/coverage-summary.py` reports line and branch totals per report. Reports remain separate because repeated or overlapping instrumentation must not be summed. CI does not enforce an arbitrary coverage percentage threshold.

Coverage counts instrumented lines and branches executed by this test workload. They do not prove that assertions are meaningful, that an acceptance criterion is satisfied, that authorization boundaries are complete, or that untested production configuration, roles, data, and failure paths are safe. Requirement status therefore comes from the RTM's requirement-linked tests and evidence, not from a coverage percentage.

## Critical-path gaps

The following gaps remain open in the current evidence:

- Staff object-level request scope: detail access now rejects staff outside their assignments, but queue scope and its acceptance criteria remain incomplete (FR-007, NFR-019).
- Production authentication: a real app-issued JWT flow is tested, but invalid issuer, audience, signature, and token expiry are not fully covered (NFR-017, NFR-018).
- State workflow: full legal/illegal transition matrix, role permutations, and exactly-one history/audit effects are not fully covered (FR-011).
- Resolution journey: requester-visible resolution state is asserted end-to-end in the test host; close-path and full role/object-scope acceptance remain incomplete (FR-013).
- Detail and resolution evidence: exhaustive request-detail fields and dedicated resolution-information assertions are absent (FR-009, FR-012).

These are requirement and security gaps, not defects that a line or branch percentage can close. Add targeted tests or record an explicit acceptance decision before changing their RTM status.

## Findings and disposition

The canonical static/security findings register is [`docs/CI/M3-CI-07-findings.md`](../../CI/M3-CI-07-findings.md). It records each repository-visible finding's source tool, severity, decision, owner, and residual-risk or debt link. Accepted and open items are expanded in the [Technical Debt Register](Technical-Debt-Register.md) and [Residual Risk Statement](Residual-Risk-Statement.md). PED §9 provides the defect and quality interpretation: [`PED-Section-9-Defects-and-Quality.md`](PED-Section-9-Defects-and-Quality.md).

Local format, vulnerability, and coverage checks are evidence for their recorded checkout only. They do not establish hosted GitHub settings, future advisory status, complete production JWT behavior, or complete requirement satisfaction. A hosted `build-and-test` job passed for commit `ab65886` on 2026-10-16 ([run #38939041921](https://github.com/Armand-JV/SEN381-Project/actions/runs/38939041921/job/117101739558?pr=143)); this is historical evidence, not verification of the final M4 commit.

## Verification evidence

- Local coverage-summary self-test: passed on 2026-10-15.
- Local full test suite: 59 passed, 0 failed, 0 skipped on 2026-10-15.
- Local format verification: passed on 2026-10-15.
- P2 human review of AI-assisted artifacts: pending; see `docs/P2-AI-Usage-Register.md`.
- Hosted CI run and artifact: pending. Add the run URL after CI executes; do not substitute local `TestResults/` for hosted evidence.

### Additional local full-suite verification — 2026-10-16

- Command: `dotnet test ".\src\CivicConnect.slnx" --configuration Release --no-restore --logger "console;verbosity=minimal"`.
- Result: solution build succeeded; 119 passed, 0 failed, 0 skipped.
- Environment: Windows, .NET 10, EF Core InMemory test database.
- Interpretation: all 119 automated tests passed in this local run.
- Limitations: this run does not establish PostgreSQL runtime behavior, production authentication behavior, hosted CI success, or complete acceptance of every requirement. Runtime warnings about the missing `wwwroot` directory and HTTPS redirect configuration appeared during testing.
