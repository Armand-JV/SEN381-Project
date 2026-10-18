# M4 Staff Object-Level Authorization Regression Evidence

- **Execution date:** 2026-10-18
- **Branch:** `test/m4-batch2-test-coverage`
- **Scope:** Staff assignment and request-state mutation authorization
- **Traceability:** FR-007, NFR-019, and the existing M3 staff-scope finding R8
- **Environment:** Local Windows development environment; automated ASP.NET Core tests using the test host and EF Core InMemory test setup

## Defect and correction

Testing before the controller correction showed that staff members outside the permitted request scope could receive a successful response when attempting assignment or state mutation.

The controller now calls `IAccessControlService.CanAccess(User, request)` after loading the request and confirming it exists, and before applying either mutation.

## Regression tests

| Test | Expected result | Result after correction |
|---|---|---|
| `AssignRequest_StaffOutsideAssignment_ReturnsForbiddenAndLeavesRequestUnchanged` | Forbidden response; state, assignee, status history, and assignment audit entries unchanged | Passed |
| `UpdateState_StaffOutsideAssignment_ReturnsForbiddenAndLeavesRequestUnchanged` | Forbidden response; state, assignee, status history, and request audit logs unchanged | Passed |

Both tests passed after the correction. The full local solution test run also passed: **121 passed, 0 failed, 0 skipped**.

## Related test-fixture correction

`AuditLogTests.UpdateState_WritesExactlyOneAuditEntry_InSameTransaction` previously used the role string `ADMIN`. The test setup was changed to `MANAGER`, a role recognized by the current access-control implementation. The focused audit test and subsequent full suite passed.

This is a test-fixture correction; it is not evidence that production administrator-role mapping has been validated.

## Verification and limitations

- `dotnet format .\src\CivicConnect.slnx --verify-no-changes --no-restore`: passed.
- `git diff --check`: passed.
- Full local solution tests: 121 passed, 0 failed, 0 skipped.
- The tests verify controller behavior and persisted state within the test environment. They do not prove PostgreSQL transaction semantics or production JWT/identity-provider behavior.
- The staff request queue remains a separate scope concern: the queue endpoint is not assignment-scoped by this correction.
- Hosted CI status and production-like database/authentication verification are not established by this local run.
- Existing local warnings include the missing `wwwroot` directory, an undetermined HTTPS redirection port, and the FluentAssertions license notice. The observer exception log is expected in its exception-isolation test.

## Disposition

The two tested mutation paths have regression coverage for staff attempting to act on a request assigned to another staff member. Do not treat this evidence as closure of the entire FR-007/NFR-019 staff-scope requirement or of the separate queue-scope gap.
