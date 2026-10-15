# M3-TEST-05 — Unit tests: notifier and dashboard aggregation (TC-U-05, TC-U-06)

Date: 2026-10-15
Branch: `feature/m3-p3-phase4`
Trace: FR-012, FR-014 (both working titles — no approved RTM artifact in this repository); DEF-006

## Basis and risk

TC-U-05 covers the Observer behaviour behind `RequestStateNotifier`: every registered
observer is called once with the exact old/new state, and one failing observer neither
blocks the rest nor escapes (DEF-006). TC-U-06 covers the FR-014 dashboard aggregation:
counts reconcile with the underlying data and filters combine correctly, with
manager-only reads enforced.

## Test records

| ID | Basis / technique | Deliberate input | Expected | Actual | Status |
|---|---|---|---|---|---|
| TC-U-05 | Observer fan-out; unit test (Moq) | Two observers; `Open` → `Assigned` | Each observer called once with same request and exact old/new states | `NotifyStateChangedAsync_CallsAllObservers` passed | PASS |
| TC-U-05 | No-observer edge; unit test | Empty observer list | Completes without error | `NotifyStateChangedAsync_WithNoObservers_DoesNotThrow` passed | PASS |
| TC-U-05 | State forwarding; unit test (Moq callback) | `InProgress` → `Resolved` | Captured old/new equal the supplied states | `NotifyStateChangedAsync_PassesCorrectStatesToObserver` passed | PASS |
| TC-U-05 / DEF-006 | Failure isolation; unit test (Moq, throwing observer) | First observer throws; second registered | Error logged; notifier completes; second observer still called once | `NotifyStateChangedAsync_WhenObserverThrows_LogsAndContinues` passed | PASS |
| TC-U-05 | Concrete observer effect; unit test (Moq logger) | Request `42`, requester `requester-7`, `Assigned` → `InProgress` | Information log contains request, requester, old/new states | `LogRequestStateObserver_LogsStateChange` passed | PASS |
| TC-U-06 | Aggregation correctness; unit test (InMemory) | 3 requests across 2 categories / 2 states | Total 3; `Open`=2, `Resolved`=1; Cat1=2, Cat2=1 | `Summary_CountsReconcile_WithUnderlyingData` passed | PASS |
| TC-U-06 | Filter combination; unit test (InMemory) | Filter by state / category / assignee | Each filter returns the 2 matching rows | `Summary_FiltersCombineCorrectly` passed | PASS |
| TC-U-06 | Access control; unit test (stub principal) | REQUESTER principal, then MANAGER/ADMIN | REQUESTER gets `Forbid`; managers get `Ok` | `Summary_NonManagerGetsForbid` passed (+ manager `Ok` in above rows) | PASS |

## Execution evidence

Environment: Windows, .NET SDK 10.0.400, EF Core InMemory test database (no network dependency).

Commands:

```text
dotnet build src/CivicConnect.slnx
Build succeeded. 0 Warning(s), 0 Error(s)

dotnet test src/CivicConnect.Tests/CivicConnect.Tests.csproj --filter 'FullyQualifiedName~RequestStateNotifierTests|FullyQualifiedName~DashboardSummaryTests'
Passed! Failed: 0, Passed: 8, Skipped: 0, Total: 8
```

## Interpretation

Both halves of M3-TEST-05 have executable evidence with real assertions (Moq `Times.Once`
verifications, captured-state equality, count reconciliation, `Forbid` vs `Ok`).
The notifier fans out exactly once per observer, isolates failures with structured logging,
and the dashboard aggregation reconciles with its data set under combined filters.

## Limitations

- Unit scope only: InMemory provider, stub principals — no JWT pipeline, no PostgreSQL
  query-plan evidence (that is M3-PERF-03 territory).
- Notification is best-effort logging, not durable delivery (TD-03, accepted).
- FR-012/FR-014 remain working titles; rows must be reconciled against the team RTM
  (M3-RTM-04) before PR closure. Dashboard age buckets deferred by team decision.
