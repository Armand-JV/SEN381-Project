# CI and security findings

## Gate policy

The CI workflow (`build-and-test` in `.github/workflows/test.yml`) fails a run when any of the following occur:

- restore or solution build failure;
- EF migration failure against the service database;
- any test failure (solution-wide, `--no-build`);
- analyzer errors (warnings are treated as errors) or format verification failure;
- a high or critical package vulnerability, or a vulnerability scan that is unavailable or unparseable.

A failed run is not rerun until green to hide flaky behavior: the cause is identified and fixed, or explicitly recorded in this file, before rerun.

## Findings record

| Tool | Severity | Finding | Decision | Owner | Residual risk |
| --- | --- | --- | --- | --- | --- |
| `dotnet format` | Medium | Existing whitespace violations in `Request.cs` and `CivicConnectDbContext.cs` | Fixed | P2 | None recorded |
| CA1707 | Warning | Underscored test method names in `CivicConnect.Tests` | Suppressed for the test assembly via `.editorconfig`; test names intentionally separate behavior, input, and expected outcome | P2 | Naming-style rule not enforced for tests |
| CA1861 | Warning | Constant array argument in EF-generated migration `InitialCreate.cs` | Suppressed for migration files; generated migration shape kept | P2 | None recorded |
| CA1870 | Warning | Non-cached `SearchValues` in `EnvLoader.cs` line 76 | Fixed: `line.AsSpan().IndexOfAny(':', '=')` | P2 | None recorded |
| `dotnet list package --vulnerable --include-transitive` | Informational | No vulnerable packages reported by configured NuGet sources in the local scan on 2026-10-14 | Accepted as current scan result; CI rescans every run | P2 | Advisories change over time; scan state at CI-run time is the gate |
| Pre-existing test defect | High | `RequestControllerTests.UpdateState_ClosedToOpen_ReturnsBadRequest` returned `Unauthorized` against the real JWT pipeline and `Forbidden` once a test scheme was registered: `AccessControlService` requires `ClaimTypes.NameIdentifier` (the JWT `sub`) that the test helper's role header never set | Fixed in test harness only: `TestAuthenticationHandler` now emits role, `sub`, and name claims from `X-Test-Role`/`X-Test-User`; test seeds the request with that `RequesterId` and uses the migrated seed Category `Id = 1` | P2 | Controller tests authenticate via `X-Test-Role`/`X-Test-User` instead of real JWTs; production JWT validation remains covered only by its own suite |
| Controller test isolation | Medium | `RequestControllerTests` created an EF context outside a DI scope; relational provider would reject cross-scope sharing | Fixed: context obtained from an injected DI scope; `TestWebApplicationFactory.CreateDbContext` removed | P2 | None recorded |
| DEF-005 history scan | High | Known `Password=postgres` / equivalent literal exists in historical `README.md` and `src/CivicConnect.Web/appsettings.json` blobs across repository history (first identified commit prefix `08f24e72c6b2`; full scan produced 65 commit/path hits, including repeated issue-document references) | Current tracked appsettings value is empty and CI uses disposable test-only credentials. Historical removal or credential rotation is not performed without repository-owner authorization | P2 / repository owner | Git history retains the old literal; rotate any credential that was ever valid and consider coordinated history rewrite only if team policy requires it |
| `.gitignore` | Low | Unrelated pre-existing working-tree change (adds `/graphify-out`) on this branch | Not part of CI implementation; left untouched | P2 | None recorded |

## Repository-level (not repository-file) security settings, 2026-10-14

Checked live via `gh api`; these are external to the repository files and are reported, not asserted as changed:

- Secret scanning: enabled; push protection: enabled; secret-scanning validity checks: disabled; non-provider patterns: disabled.
- Dependabot security updates (auto-fix PRs): disabled. The `dependabot.yml` file in this change enables Dependabot *update* PRs for nuget and github-actions (weekly). "Dependabot alerts" (vulnerability-alerts API) reports disabled; enable it through GitHub settings, not a repository file.
- `main` branch ruleset: two required approvals, required linear history, delete and non-fast-forward protections. **No required-status-check rule exists.** Making the CI workflow a required check requires a live ruleset/settings change (authorized); the check name must match the `build-and-test` job.
- Blocked-PR evidence (M3-CI-03) requires a real failing run that blocks merge; not present in this change.

This file records repository-visible findings only. Hosted run URLs and required-status-check state require live evidence and are not asserted here.
