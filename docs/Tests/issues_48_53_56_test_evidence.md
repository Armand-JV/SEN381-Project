# CivicConnect Test Evidence — Issues #48, #53–#56

**Test date:** 15 October 2026  
**Local environment:** Windows 11 Pro, .NET SDK 10.0.400, EF Core InMemory provider 10.0.12  
**Automated test command:**

```text
dotnet test src/CivicConnect.slnx --configuration Debug --no-restore --logger "console;verbosity=minimal"
Passed: 75, Failed: 0, Skipped: 0
```

## Test records

| ID | Basis / selection | Level / technique | Deliberate input | Expected before execution | Actual | Evidence | Trace | Status |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| TC-U-03a | State-transition table; high-impact lifecycle guard | Unit / state transition | `Open` → `Assigned` | Allowed | `true` | `RequestTransitionPolicyTests.IsAllowed_OpenToAssigned_ReturnsTrue` | FR-009, #48 | PASS |
| TC-U-03b | State-transition table; terminal-state regression | Unit / state transition | `Closed` → `Open` | Rejected | `false` | `RequestTransitionPolicyTests.IsAllowed_ClosedToOpen_ReturnsFalse` | FR-009, #48 | PASS |
| TC-U-04 | Role policy; requester must not reject a request | Unit / role decision | Authenticated `REQUESTER`, `Open` → `Rejected`, nonblank reason | Rejected with an error | Rejected with nonblank error | `RequestTransitionPolicyTests.TryValidate_RequesterRole_FailsStaffTransition` | NFR-018, #48 | PASS |
| TC-I-01 | Submission and initial-history persistence; DEF-001/DEF-002 regression | API integration | Authenticated requester posts valid category-1 request | 201, response DTO, one Open request, one linked initial history row | Expected DTO fields and one linked row observed | `RequestControllerTests.CreateRequest_ValidSubmission_ReturnsCreatedDtoAndPersistsInitialHistory` | FR-001, DEF-001, DEF-002, #53 | PASS locally |
| TC-I-02 | API input failure; prevent invalid reference persistence | API integration / negative | Authenticated requester posts `CategoryId = Int32.MaxValue` | 400; no request/history row for input | 400; no matching request/history row observed | `RequestControllerTests.CreateRequest_UnknownCategory_ReturnsBadRequestAndPersistsNothing` | FR-001, #54 | PASS locally |
| TC-I-03 | Authentication boundary | API integration / negative | Anonymous `POST /api/requests` | 401; request absent | 401; request absent | `RequestControllerTests.CreateRequest_AnonymousCallerReturnsUnauthorized` | NFR-017, #54 | PASS locally |
| TC-I-04 | Controller/persistence/Observer boundary | API integration | Assigned request, assigned staff, `Assigned` → `InProgress` | 200; exactly one history row; observer receives same request and states | 200; persisted history and observer Moq verification passed | `RequestControllerTests.UpdateState_ValidTransition_NotifiesObserver` | FR-009, FR-012, #55 | PASS locally |
| TC-I-05 | Invalid transition must have no side effect | API integration / negative state transition | Open request, `Open` → `Resolved` | 4xx; state remains Open; no history | 400; state Open; no history row | `RequestControllerTests.UpdateState_IllegalTransition_LeavesRequestUnchanged` | FR-009, #55 | PASS locally |
| TC-N-01 | Role x endpoint decision | API integration / unauthorised | Requester calls `GET /api/staff/requests` | 403 | 403 | `RequestControllerTests.GetStaffRequests_Requester_ReturnsForbidden` | NFR-018, #56 | PASS locally |
| TC-N-02 | Ownership isolation | API integration / unauthorised | Requester A reads requester B's request | 403 or 404 | 403; target record still exists | `RequestControllerTests.GetRequest_OtherRequester_ReturnsForbidden` | NFR-019, #56 | PASS locally |
| TC-N-03 | Assigned-only staff scope | API integration / unauthorised | Staff B reads request assigned to staff A | 403; data unchanged | 403; request remained Assigned; no history row | `RequestControllerTests.GetRequest_StaffOutsideAssignment_ReturnsForbidden` | FR-013, NFR-019, #56 | PASS locally |

## Interpretation

These cases cover high-risk lifecycle, authentication, role, ownership, scope, persistence, and Observer paths. TC-N-01 overlaps the pre-existing requester-staff endpoint check; TC-N-02 overlaps the pre-existing cross-requester check; TC-N-03 overlaps the pre-existing out-of-scope staff check. They are counted once each here, not as additional cases.

## Relational database limitation and CI evidence

`TestWebApplicationFactory` selects PostgreSQL when `ConnectionStrings__DefaultConnection` is set; otherwise local tests use a unique EF Core InMemory database. The local result above does **not** prove foreign-key enforcement. `.github/workflows/test.yml` starts PostgreSQL, applies migrations, and sets that connection string before test execution. CI must be green for this commit before TC-I-01 is treated as complete relational-database evidence.
