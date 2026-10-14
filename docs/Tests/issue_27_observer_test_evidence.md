# Issue #27 — Observer test evidence

Date: 2026-10-14  
Branch: `feature/-FR-012-Concrete-state-change-observers`  
Trace: FR-012 working title; M2 Observer decision; TC-U-05; DEF-006; TC-I-04

## Basis and risk
`RequestStateNotifier` fans out state changes through `IRequestStateObserver`. Before this change, the concrete observer was a no-op and an observer exception escaped after `SaveChangesAsync`, potentially returning an error for an already-committed state change.

Risk selected: observer behavior and post-commit failure isolation. A durable outbox/retry mechanism is not part of this change.

## Test records

| ID | Basis / technique | Deliberate input | Expected | Actual | Status |
|---|---|---|---|---|---|
| TC-U-05 | Observer fan-out; unit test | Two observers; `Open` → `Assigned` | Each observer called once with same request and exact old/new states | `NotifyStateChangedAsync_CallsAllObservers` passed | PASS |
| TC-U-05b | Concrete observer effect; unit test | Request `42`, requester `requester-7`, `Assigned` → `InProgress` | Information log contains request, requester, old state, new state | `LogRequestStateObserver_LogsStateChange` passed | PASS |
| DEF-006 | Failure isolation; unit test | First observer throws; second observer registered | Error logged; notifier completes; second observer still called once | `NotifyStateChangedAsync_WhenObserverThrows_LogsAndContinues` passed | PASS |
| TC-I-04 / DEF-006 | API/integration regression | Signed JWT for owner `alice`; authenticated `PUT /api/requests/{id}/state` with test observer throwing before concrete log observer | HTTP 200; state/history committed; subsequent concrete observer emits Information log | `UpdateState_ObserverThrows_ReturnsOkAndPreservesStateChange` passed | PASS |
| TC-I-04 | API/integration success | Signed JWT for owner `alice`; `Open` → `InProgress` | HTTP 200; one history row records correct old/new states | `UpdateState_ValidTransition_ReturnsOkAndPersistsHistory` passed | PASS |
| FR-012 / DI | DI resolution; integration test | Application service provider | Concrete `LogRequestStateObserver` and `RequestStateNotifier` resolve | `Program_Registration_IncludesConcreteObserver` passed | PASS |

## Execution evidence

Environment: Windows 11 Pro, .NET SDK 10.0.400, EF Core InMemory test database.

Commands:

```text
dotnet build CivicConnect.slnx
Build succeeded. 0 Warning(s), 0 Error(s)

dotnet test CivicConnect.Tests/CivicConnect.Tests.csproj --filter 'FullyQualifiedName~RequestStateNotifierTests|FullyQualifiedName~RequestControllerTests'
Passed! Failed: 0, Passed: 9, Skipped: 0, Total: 9
```

## Interpretation
The Observer decision has executable evidence: DI resolves a concrete observer, state changes produce structured logs, every observer receives the event, and an observer failure is isolated after persistence. `RequestStateNotifier` catches and logs each observer failure, then continues with remaining observers.

## Limitations
Logs are best-effort and not durable notification delivery. No outbox, retry, delivery acknowledgement, or external notification provider is tested. FR-012 wording remains a working title because no approved RTM artifact was present in this repository; requirement wording must be reconciled against the team RTM before PR closure.
