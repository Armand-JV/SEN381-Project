# M3-CI-07 Static/security findings

**Canonical register:** this file records repository-visible findings for M3-CI-07. Scan results are point-in-time evidence, not a promise that future advisories or hosted settings remain unchanged. Owners must review each disposition before release acceptance.

## Disposition vocabulary

- **Fixed:** repository change removes the finding for the stated scope.
- **Mitigated:** controls reduce the finding, but residual exposure remains.
- **Accepted:** team records why the finding remains and assigns follow-up in the [Residual Risk Statement](../Residual-Risk-Statement.md) or [Technical Debt Register](../Technical-Debt-Register.md).

## Findings record

| ID | Tool / source | Severity | Finding | Decision | Owner | Residual risk / debt |
|---|---|---|---|---|---|---|
| FIND-001 | `dotnet format` | Medium | Existing whitespace violations in `Request.cs` and `CivicConnectDbContext.cs`. | Fixed | P2 | None. Verify with the repository format gate. |
| FIND-002 | CA1707 | Warning | Underscored test method names in `CivicConnect.Tests`. | Mitigated | P2 | Test-only naming exception remains in `.editorconfig`; naming-style rule is not enforced for tests. |
| FIND-003 | CA1861 | Warning | Constant array argument in EF-generated `InitialCreate.cs`. | Mitigated | P2 | Generated migration shape remains excepted in `.editorconfig`; no production source suppression. |
| FIND-004 | CA1870 | Warning | Non-cached `SearchValues` in `EnvLoader.cs` line 76. | Fixed | P2 | None; replaced with `line.AsSpan().IndexOfAny(':', '=')`. |
| FIND-005 | `dotnet list package --vulnerable --include-transitive` | Informational | No vulnerable packages reported by configured NuGet sources in local scan on 2026-10-14. | Accepted | P2 | [RR-SEC-02](../Residual-Risk-Statement.md#rr-sec-02); advisories change over time and CI rescans every run. |
| FIND-006 | Pre-existing test defect / JWT test harness | High | `RequestControllerTests.UpdateState_ClosedToOpen_ReturnsBadRequest` returned `Unauthorized` against the real JWT pipeline and then `Forbidden` when a test scheme was registered because `AccessControlService` requires `ClaimTypes.NameIdentifier` (`sub`). | Fixed | P2 | Test harness uses `X-Test-Role`/`X-Test-User`; production JWT validation remains outside this suite. See [RR-SEC-04](../Residual-Risk-Statement.md#rr-sec-04). |
| FIND-007 | Controller test isolation | Medium | `RequestControllerTests` created an EF context outside a DI scope; relational providers reject cross-scope sharing. | Fixed | P2 | None recorded. |
| FIND-008 | DEF-005 history scan | High | Historical Git blobs retain a `Password=postgres` or equivalent literal; scan produced 65 commit/path hits, first identified by commit prefix `08f24e72c6b2`. | Mitigated | P2 / repository owner | [TD-SEC-01](../Technical-Debt-Register.md#td-sec-01) and [RR-SEC-01](../Residual-Risk-Statement.md#rr-sec-01); current tracked appsettings value is empty, but history remains. |
| FIND-009 | `gh api` repository settings check, 2026-10-14 | Medium | Secret-scanning validity checks and non-provider patterns were disabled; Dependabot security alerts/auto-fix were disabled; no required-status-check rule existed in the main ruleset. | Accepted | P2 / repository owner | [RR-SEC-03](../Residual-Risk-Statement.md#rr-sec-03) and [TD-SEC-02](../Technical-Debt-Register.md#td-sec-02); hosted settings require authorized changes and live evidence. |

## Evidence limits

- `dotnet list package` and local self-tests do not replace hosted CI evidence. The workflow gate is defined in [`.github/workflows/test.yml`](../../.github/workflows/test.yml).
- Coverage counts do not prove requirement satisfaction, authorization completeness, production JWT behavior, or safe failure paths. See [PED v3 §5](../Milestone%203/PED-Section-5-CI-and-Coverage.md).
- Historical credentials are not removed or rotated by this documentation change. Coordinate any history rewrite or credential action with the repository owner.
- Hosted GitHub settings are reported as findings, not asserted as changed by this branch.

## Related traceability

- [PED v3 §5 — CI and automated verification](../Milestone%203/PED-Section-5-CI-and-Coverage.md#findings-and-disposition)
- [PED v3 §9 — defects and quality interpretation](../Milestone%203/PED-Section-9-Defects-and-Quality.md)
- [RTM-M3](../RTM-M3.md)
- [M3 issue M3-CI-07](../CivicConnect_M3_Issues.md#m3-ci-07-staticsecurity-findings-log-and-handling)
