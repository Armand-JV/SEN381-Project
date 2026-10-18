# CivicConnect Product Error/Debugging Document (PED) v3.0

**Release Candidate — M3**

**Version:** 3.0  
**Date:** 2026-10-16  
**Repository:** https://github.com/Armand-JV/SEN381-Project  
**Tag/Commit:** `a1e189b` (Riegardt Phase 6: management slice, release review and readiness evidence — #138)  
**PED v3.0:** `docs/Milestone 3/PEDv3.0.md`  
**RTM:** `docs/Milestone 3/RTM-M3.md`  
**CI:** `.github/workflows/test.yml`  

---

## Team members

| Role | Person | Rubric areas | Slices owned |
|---|---|---|---|
| Quality & Verification Lead | Ditheto | A (M2 conformance), D (black-box/integration/system), E (RTM/defects/traceability) | Requester slice FR-001–006 |
| Construction, CI & Security Lead | Armand | B (collaborative construction), C (CI & quality gates) | Staff slice FR-007–013, NFR-017–019 |
| Release, Performance & Operations Lead | Riegardt | F (performance/staging), G (release review, residual risk) | Management slice FR-014–016, NFR-020, NFR-024 |

**Submission date:** 2026-10-16  
**Classification: CONDITIONALLY READY** (see Section 10)

---

## 1. Release Candidate Scope and Submission Information

### 1.1 In-scope requirements (frozen)

The M3 release candidate covers the agreed core scope. Deferred items are recorded in the Technical Debt Register with reasons.

| ID | Requirement | Owner | RTM status | Evidence |
|---|---|---|---|---|
| FR-001 | Submit validated service request | Ditheto | Implemented | `docs/Tests/requester-slice.md`; `RequesterSliceTests.cs` |
| FR-002 | Category selection and category-specific validation | Ditheto | Implemented | `docs/Tests/issue_27_observer_test_evidence.md`; `CategoryValidationContextTests.cs` |
| FR-003 | Unique request reference | Ditheto | Verified | Numeric `Request.Id` accepted as reference (Riegardt decision) |
| FR-004 | Requester views own request and status | Ditheto | Verified | `RequesterSliceTests.cs`, `GetMyRequests_*` |
| FR-005 | Requester views status history timeline | Ditheto | Verified | `RequesterSliceTests.GetHistory_*` |
| FR-006 | Requester feedback on completed request | Ditheto | Implemented | `RequesterSliceTests.Feedback_*`; `FeedbackController.cs` |
| FR-007 | Staff view relevant requests | Armand | Implemented | `docs/Tests/github_issues_22-25_test_evidence.md`; `RequestControllerTests.GetStaffRequests_*` |
| FR-008 | Filter/search/sort requests | Armand | Implemented | `GetStaffRequests_CombinesFiltersAndPaging`; |
| FR-009 | Staff view full request details | Armand | Implemented| `RequestControllerTests.GetRequest_SeededRequestIncludesComment` |
| FR-010 | Assign request to staff | Armand | Verified | `RequestControllerTests.AssignRequest_*` |
| FR-011 | Public and internal comments | Armand | Implemented | `RequestControllerTests.AddComment_*`; full matrix not covered |
| FR-012 | State update transitions | Armand | Implemented | Partial; full matrix not covered |
| FR-013 | Staff scope restriction | Armand | Implemented | `GetRequest_StaffOutsideAssignment_ReturnsForbidden` |
| NFR-017 | Authentication | Armand | Implemented | `Program.cs` JWT bearer; test handler limitation (RR-SEC-04) |
| NFR-018 | Role-based authorization | Armand | Implemented | Role attributes on controllers; `AccessControlService` |
| NFR-019 | Object-level access control | Armand | Implemented | Requester ownership verified; staff-assign scope tested |
| FR-014 | Management dashboard | Riegardt | Verified | `DashboardSummaryTests.cs`; `docs/Riegardt-Decisions.md` M3-PERF-01 |
| FR-015 | Management oversight: stalled + reassign | Riegardt | Verified | `ManagementTests.cs`; `docs/Riegardt-Decisions.md` FR-015 |
| FR-016 | Category administration | Riegardt | Verified | `ManagementTests.cs`; `docs/Riegardt-Decisions.md` FR-016 |
| NFR-020 | Audit trail | Riegardt | Verified | `AuditLogTests.cs`; `docs/Riegardt-Decisions.md` NFR-020 |
| NFR-024 | Responsive everyday operations | Riegardt | Verified | TC-P-01: p95 23.71ms (target 3s) — PASS |

### 1.2 Deferred requirements

| ID | Requirement | Reason | Debt link |
|---|---|---|---|
| FR-008 search | Full-text search on requests | Deferred by team decision | `docs/Technical-Debt-Register.md` TD-10 |
| Dashboard age buckets | Request age bucketing in summary | Deferred by user decision | `docs/Technical-Debt-Register.md` TD-10 |
| Outbox/retry for observers | Durable notification delivery | Deferred beyond M3 (accepted) | `docs/Technical-Debt-Register.md` TD-03 |

### 1.3 Submission references

| Artefact | Location |
|---|---|
| Source code | `src/` (4 projects: Domain, Application, Web, Tests + Perf) |
| CI workflow | `.github/workflows/test.yml` |
| RTM | `docs/RTM-M3.md` |
| Test evidence | `docs/Tests/` |
| Registers (defects, debt, risks, AI) | `docs/` root |
| Staging scripts | `scripts/staging-e2e.ps1`, `scripts/staging-smoke.ps1` |
| Performance harness | `src/CivicConnect.Perf/` |

---

## 2. M2 Baseline → Implementation Conformance Table

Adapted from `docs/Milestone 3/CivicConnect M2 ADR Transition Tables (M2 to M3).md`.

| ADR | M2 decision | Conformance | Evidence | Status |
|---|---|---|---|---|
| ADR-01 | Layered: Domain / Application / Web / Tests | Partial — `IRequestRepository`/`ICategoryRepository` interfaces exist and implemented; `RequestsController` uses `DbContext` directly; Application service layer | `src/CivicConnect.Application/`, `src/CivicConnect.Web/Controllers/RequestsController.cs` | Significant deviation (Section 3 / ADR-01) |
| ADR-02 | PostgreSQL + EF Core, code-first migrations | Conformant on schema | `CivicConnectDbContext.cs`, `Migrations/`, `db_schema.sql` | Refined — integrity gaps addressed (cascade restrict, concurrency token in Riegardt decisions) |
| ADR-03 | .NET 10 / ASP.NET Core / Swagger / xUnit, GitHub Actions | Conformant on versions | `Directory.Build.props`, CI workflow, project files | Refined — config, CI gates extended |
| ADR-04 | Observer pattern for state changes | Interface + `RequestStateNotifier` exist; concrete `LogRequestStateObserver` now registered and tested | `RequestStateNotifier.cs`, `Observer/` in Application | Completed — concrete observer registered (Issue #27) |
| ADR-05 | Strategy pattern for category validation | Interface + `CategoryValidationContext` exist; `Category1ValidationStrategy` now registered | `Strategy/`, `Program.cs` registrations | Completed — concrete strategy registered |
| ADR-06 | REST API via ASP.NET Core controllers | Endpoints for all slices | `Controllers/`, README API list | Updated — JWT auth, role attributes, DTOs added |

### Significant deviations requiring ADR records

1. **Repository abstraction bypass (ADR-01):** `RequestsController` uses `CivicConnectDbContext` directly. Accepted simplification, documented as TD-01. No repository implementations added.
2. **Production JWT validation not exercised by tests:** `TestHeaderAuthenticationHandler` proves controller wiring only (RR-SEC-04). Production JWT validation is not tested.
3. **API path version:** PED v2 planned `/api/v1/`; implementation uses `/api`. Accepted change, documented in RTM evidence limits.
4. **State pattern:** PED v2 proposed State objects; implementation uses central `RequestStateTransitionPolicy` table. Accepted change, tested.

---

## 3. Controlled Changes

### 3.1 ADR-01 change record — DbContext directly in controller

| Stage | Content |
|---|---|
| **Original decision (M2)** | Repository interfaces in Application; thin controllers |
| **Implementation evidence** | `IRequestRepository`/`ICategoryRepository` declared in `CivicConnect.Application/Interfaces/` but no implementations exist; nothing uses them |
| **Deviation found** | `RequestsController` injects `CivicConnectDbContext` directly; business logic (validation, state change, history, notification) in controller |
| **Revised decision** | Accept DbContext-as-UoW as a deliberate simplification (Option B from M2 ADR document). Delete unused repository interfaces or implement repositories per ADR-M3-02 if testability demands. |
| **Recorded as** | Technical Debt TD-01 |
| **Status** | Accepted |

### 3.2 State-pattern change — central transition table

| Stage | Content |
|---|---|
| **Original decision (M2)** | State pattern objects |
| **Implementation evidence** | `RequestStateTransitionPolicy.cs` — static `PermittedTransitions` dictionary |
| **Revised decision** | Central transition table is simpler and equally testable |
| **Consequence** | RTM design-decision column re-pointed; behavior tested via `RequestTransitionPolicyTests` and controller tests |
| **Status** | Accepted; PED conformance row updated |

### 3.3 Authentication/authorization change

| Stage | Content |
|---|---|
| **Original decision (M2)** | RBAC with real identity provider |
| **Implementation evidence** | JWT bearer authentication in `Program.cs`; `TestHeaderAuthenticationHandler` for test environments only; role attributes `[Authorize(Roles = "STAFF,MANAGER")]` |
| **Revised decision** | JWT bearer for production; test-header scheme only in `Testing` environment (Development + `DemoAuth:Enabled`) |
| **Consequence** | Controller authorization wiring verified; production JWT validation not exercised (RR-SEC-04) |
| **Status** | Accepted with residual risk |

---

## 4. Construction and Configuration

### 4.1 Project structure

```
src/
├── CivicConnect.Domain/       — Entities, Enums, POCOs
├── CivicConnect.Application/  — Policies, Services, Patterns (Observer, Strategy)
├── CivicConnect.Web/          — Controllers, DbContext, Migrations, SPA
├── CivicConnect.Tests/        — Unit + integration tests
└── CivicConnect.Perf/         — NBomber load tests
```

Dependencies flow Web → Application → Domain. Web also references Domain directly. Tests reference all three.

### 4.2 Domain layer

Entities (`src/CivicConnect.Domain/Entities/`):
- `Request` — `Id`, `Title`, `Description`, `CategoryId`, `State`, `CreatedAt`, `RequesterId`, `AssigneeId`, navigation collections
- `Category` — `Id`, `Name`, `Description`
- `Comment` — `Id`, `RequestId`, `Text`, `CreatedAt`, `CreatedBy`, `IsInternal`
- `StatusHistory` — `Id`, `RequestId`, `PreviousState`, `NewState`, `ChangedAt`, `ChangedBy`, `Reason`
- `AuditLog` — `Id`, `Actor`, `Action`, `EntityType`, `EntityId`, `BeforeSummary`, `AfterSummary`, `Timestamp`, `CorrelationId`
- `AuditEntry` — deprecated; side-by-side with `AuditLog` (TD-04)
- `Feedback` — `Id`, `RequestId`, `Rating` (1–5), `Comment` (≤2000), `CreatedAt`, `CreatedBy`

`RequestState` enum: `Open, Assigned, InProgress, Resolved, Closed, Cancelled, Rejected`

### 4.3 Application layer

**Policies:**
- `RequestStateTransitionPolicy` (`src/CivicConnect.Application/Policies/`) — central transition table; `RequiresReason` for Rejected/Cancelled/Closed
- Permitted transitions:
  - Open → Assigned, Rejected, Cancelled
  - Assigned → InProgress, Rejected, Cancelled
  - InProgress → Resolved, Cancelled
  - Resolved → Closed
  - Closed → (terminal)
  - Cancelled → (terminal)
  - Rejected → (terminal)

**Patterns:**
- Observer: `IRequestStateObserver` + `RequestStateNotifier` (fans out sequentially; failures isolated)
- Strategy: `CategoryValidationContext` selects `ICategoryValidationStrategy` by category ID; fallback non-empty title/description validation

### 4.4 Web/config layer

**DI registrations (`Program.cs`):**
```
- DbContext<CivicConnectDbContext> (Npgsql)
- CategoryValidationContext (scoped)
- ICategoryValidationStrategy → Category1ValidationStrategy (scoped)
- IRequestStateObserver → LogRequestStateObserver (scoped)
- RequestStateNotifier (scoped)
- IAccessControlService → AccessControlService (scoped)
```

**Environment configuration:**
- `EnvLoader` loads `.env` from content/current/base directories upward without overwriting existing process variables
- DB precedence: `DB_*` vars → `ConnectionStrings:DefaultConnection` → local PostgreSQL defaults
- JWT from `JWT_ISSUER`, `JWT_AUDIENCE`, `JWT_SECRET`

**API endpoints:**

| Endpoint | Roles | Description |
|---|---|---|
| `POST /api/requests` | Auth | Submit request (validated, requester from principal) |
| `GET /api/requests/{id}` | REQ own / STF / MGR | Request detail; internal comments hidden from requesters |
| `GET /api/requests/mine` | REQ | Own requests, paged |
| `GET /api/requests/{id}/history` | owner / STF / MGR | Status history timeline |
| `PUT /api/requests/{id}/state` | STF / MGR | State transition (reason required for Reject/Cancel) |
| `PUT /api/requests/{id}/assign` | STF / MGR | Assign to staff, Open→Assigned |
| `POST /api/requests/{id}/comments` | REQ public/own / STF | Public/internal comments |
| `POST /api/requests/{id}/feedback` | REQ owner, Resolved/Closed | Rating 1–5, one per request |
| `GET /api/categories` | Auth | Seeded category list |
| `GET /api/staff/requests` | STF / MGR | Queue with filters + paging |
| `GET /api/reports/requests-summary` | MGR | Counts by state/category |
| `GET /api/audit` | MGR | Append-only audit trail |
| `GET /api/management/stalled` | MGR | Open/Assigned older than N days |
| `PUT /api/management/requests/{id}/reassign` | MGR | Reassign with history + audit |
| `POST /api/management/categories` | MGR | Create category |
| `PUT /api/management/categories/{id}` | MGR | Rename category |
| `GET /health` | Anonymous | Database connectivity |
| `GET /api/version` | Anonymous | Build/commit/environment |

**Swagger:** Enabled in Development and Staging (`Program.cs`); never Production.

**Frontend:** React 19 + Vite SPA (`src/CivicConnect.Web/ClientApp/`), built output served from `wwwroot` with SPA fallback.

---

## 5. CI and Automated Verification

### 5.1 CI workflow

`.github/workflows/test.yml` runs on push/PR to `main`:

1. Checkout repository
2. Setup Node.js 22 (for frontend build)
3. Build frontend (`npm ci` + `npm run build`)
4. Setup .NET 10
5. Restore dependencies
6. Build solution (Release, warnings-as-errors)
7. Install EF tool
8. Apply database migrations (PostgreSQL service on port 5432)
9. Verify formatting (`dotnet format --verify-no-changes`)
10. Check vulnerable packages (`scripts/check-vulnerabilities.py`)
11. Validate coverage script self-test
12. Test solution with TRX + XPlat coverage (Cobertura format)
13. Summarise Cobertura coverage (`scripts/coverage-summary.py`)
14. Upload test results artifact

### 5.2 Quality gates

| Gate | Mechanism | Blocks merge |
|---|---|---|
| Build | `dotnet build` (warnings-as-errors) | Yes |
| Format | `dotnet format --verify-no-changes` | Yes |
| Tests | `dotnet test` (all must pass) | Yes |
| Vulnerabilities | `scripts/check-vulnerabilities.py` (rejects high/critical) | Yes |
| PostgreSQL | Migration applies, Requests table confirmed | Yes |

### 5.3 Branch protection

`.github/Main Branch protection.json`:
- Requires 2 non-author approvals
- Dismiss stale reviews on push
- Require last push approval
- Required linear history
- Copilot code review on push
- No force pushes to `main`

### 5.4 Coverage interpretation

CI collects Coverlet Cobertura output during `dotnet test`. `scripts/coverage-summary.py` reports line/branch totals per report. CI does **not** enforce an arbitrary coverage percentage threshold.

Coverage counts instrumented lines/branches executed. It does **not** prove:
- Assertions are meaningful
- Acceptance criteria are satisfied
- Authorization boundaries are complete
- Untested production configuration is safe

Requirement status comes from the RTM, not coverage percentages. See `docs/Milestone 3/PED-Section-5-CI-and-Coverage.md`.

### 5.5 Critical-path gaps

Per `docs/Milestone 3/PED-Section-5-CI-and-Coverage.md`:

- Staff object-level request scope: not blocked or tested (FR-007, NFR-019)
- Production authentication: issuer/audience/signature not exercised (NFR-017, NFR-018)
- State workflow: full transition matrix not fully covered (FR-011)
- Resolution journey: requester-visible state after staff resolution lacks E2E assertion (FR-013)
- Detail and resolution evidence: exhaustive field assertions absent (FR-009, FR-012)

### 5.6 Static/security findings

Canonical register: `docs/CI/M3-CI-07-findings.md`. See Section 9 for discharge.

| ID | Tool | Severity | Finding | Disposition |
|---|---|---|---|---|
| FIND-001 | dotnet format | Medium | Whitespace violations | Fixed |
| FIND-002 | CA1707 | Warning | Underscored test names | Mitigated (.editorconfig exception) |
| FIND-003 | CA1861 | Warning | Constant array in migration | Mitigated (.editorconfig exception) |
| FIND-004 | CA1870 | Warning | Non-cached SearchValues | Fixed |
| FIND-005 | dotnet list package | Informational | No vulnerable packages | Accepted (monitoring, RR-SEC-02) |
| FIND-006 | Test harness | High | JWT test fix failures | Fixed |
| FIND-007 | Test isolation | Medium | EF context cross-scope | Fixed |
| FIND-008 | History scan | High | Historical credential in Git | Mitigated (RR-SEC-01, TD-SEC-01) |
| FIND-009 | gh settings | Medium | Unrelated .gitignore change | Out of scope |
| FIND-010 | GitHub settings | Medium | Missing security settings | Accepted (RR-SEC-03) |

### 5.7 Verification evidence

| Check | Result | Date |
|---|---|---|
| Local coverage-summary self-test | Passed | 2026-10-15 |
| Local full test suite | 118 passed, 0 failed | 2026-10-15 |
| Local format verification | Passed | 2026-10-15 |
| Armand human review | Pending | — |
| Hosted CI run | Pending | — |

---

## 6. Quality Strategy

### 6.1 Risk-based test selection

Per `docs/Milestone 3/SEN381_M3_Work_Split.md` and `docs/CivicConnect_M3_Issues.md` (M3-TEST-02).

Top risks prioritized by likelihood × impact:

| Risk | Priority | Verification approach | Test cases |
|---|---|---|---|
| Authentication bypass (DEF-003/004) | High | Integration tests with auth boundary | TC-I-03, TC-N-01, TC-N-02 |
| Illegal state transition | High | State transition tests | TC-U-03a/b, TC-I-05, TC-24-05..07 |
| Data persistence failures | High | Integration tests on PostgreSQL | TC-I-01, TC-I-02 |
| Authorization bypass | Medium | Negative/unauthorized suite | TC-N-01..03 |
| Observer failure isolation | Medium | Moq-based failure tests | TC-U-05 (DEF-006) |
| Dashboard filter correctness | Medium | Count reconciliation tests | TC-U-06 |
| Performance regression | Medium | Load test on dashboard | TC-P-01 |

### 6.2 Test allocation

| Category | Lead | Technique | Test classes (17) | Count |
|---|---|---|---|---|
| Unit/component | All | EP/BVA, state transition, Moq | `DomainEntityTests`, `RequestTransitionPolicyTests`, `RequestStateNotifierTests`, `CategoryValidationContextTests`, `UserStoreTests` | 67 |
| Black-box functional | Ditheto | EP/BVA, state transition, decision table | `RequesterSliceTests` | 15 |
| API/integration | Armand | Success + negative cases | `RequestControllerTests`, `AuthServiceTests`, `PrivilegedProvisioningTests` | 51 |
| Negative/unauthorized | Armand | Role-based, ownership, scope | Subset of `RequestControllerTests`, `AuthServiceTests` | 13 |
| E2E/system | Riegardt | Critical journey on staging | `E2EJourneyTests` | 2 |
| Performance | Riegardt | NBomber load test | `CivicConnect.Perf` | 1 |
| **Total** | | | | **118** |

### 6.3 Meaningful Test Rule

A test counts only if it has:
1. Traceable basis (requirement/risk)
2. Purpose (technique)
3. Deliberate input data
4. Defined expected result
5. Real assertion
6. Execution evidence
7. Interpretation

### 6.4 Evidence sources

| Test evidence file | Trace |
|---|---|
| `docs/Tests/requester-slice.md` | TC-U-05/06 (Requester slice, 2026-10-15) |
| `docs/Tests/github_issues_22-25_test_evidence.md` | TC-22-01..25-06 (Staff slice, 2026-10-13) |
| `docs/Tests/issues_48_53_56_test_evidence.md` | TC-U-03..04, TC-I-01..05, TC-N-01..03 (2026-10-14) |
| `docs/Tests/m3-test-05-evidence.md` | Notifiers + dashboard (2026-10-15) |
| `docs/Tests/issue_27_observer_test_evidence.md` | Observer fan-out + failure isolation (2026-10-14) |

---

## 7. Black-Box, Integration, and System Tests

### 7.1 Unit tests (TC-U-03..06)

From `docs/Tests/issues_48_53_56_test_evidence.md` and `docs/Tests/m3-test-05-evidence.md`:

| ID | Basis | Result |
|---|---|---|
| TC-U-03a | State transition Open→Assigned allowed | PASS |
| TC-U-03b | State transition Closed→Open rejected | PASS |
| TC-U-04 | Role policy: requester cannot reject | PASS |
| TC-U-05 | Observer fan-out, failure isolation | 8/8 PASS |
| TC-U-06 | Dashboard counts + filters | 3/3 PASS |

### 7.2 Black-box functional (TC-B)

| Technique | Basis | Cases |
|---|---|---|
| Equivalence Partitioning / BVA | FR-001 submission fields | TC-B-01..04 (Ditheto, requester slice) |
| State Transition Testing | FR-011 status changes | TC-B-05..07 (legal/illegal transitions) |
| Decision Table | NFR-018 role×action | TC-B-08..10 (who can do what) |

### 7.3 API/integration tests

| ID | Basis | Input | Expected | Actual | Status |
|---|---|---|---|---|---|
| TC-I-01 | FR-001 submission + initial history | Valid category-1 request, authenticated requester | 201 DTO, one Open request, one linked history row | Confirmed | PASS (local) |
| TC-I-02 | FR-001 unknown category | CategoryId = MaxValue | 400, no persistence | Confirmed | PASS (local) |
| TC-I-03 | NFR-017 anonymous | Anonymous POST /api/requests | 401, no request | Confirmed | PASS (local) |
| TC-I-04 | FR-009 + FR-012 | Assigned→InProgress, observer | 200, exactly one history row, observer notified | Confirmed | PASS (local) |
| TC-I-05 | FR-009 illegal transition | Open→Resolved | 400, state unchanged, no history | Confirmed | PASS (local) |

CI must be green against PostgreSQL for TC-I-01 as complete relational evidence (InMemory does not enforce FKs).

### 7.4 Negative/unauthorized tests

| ID | Basis | Input | Expected | Actual | Status |
|---|---|---|---|---|---|
| TC-N-01 | NFR-018 | Requester calls GET /api/staff/requests | 403 | Confirmed | PASS |
| TC-N-02 | NFR-019 | Requester A reads requester B's request | 403 or 404 | 403, target still exists | PASS |
| TC-N-03 | FR-013, NFR-019 | Staff B reads request assigned to staff A | 403, unchanged | Confirmed | PASS |

### 7.5 E2E journeys (TC-S-01, TC-S-02)

From `src/CivicConnect.Tests/E2EJourneyTests.cs`:

| ID | Journey | Evidence |
|---|---|---|
| TC-S-01 | Submit → staff assigns → resolves → requester sees update + history at DB level | `SubmitAssignResolve_RequesterSeesUpdate` — 200/0 history rows match expected states |
| TC-S-02 | Resolved request → feedback → dashboard reflects it | `ResolvedFeedback_DashboardReflectsIt` — 201 feedback, 200 dashboard, total=1, byState Resolved=1 |

Staging run (M3-TEST-15, Issue #59): `STAGING-E2E PASS: TC-S-01 green on commit ec9cb7a` via `scripts/staging-e2e.ps1`.

### 7.6 Postman test cases

`docs/Postman_Request_Comments_Test_Cases.md` — 14 manual HTTP API test cases (TC-01..14) covering comment creation, visibility, ownership, authorization, and error paths.

---

## 8. Performance and Reliability

### 8.1 Performance operation

From `docs/Riegardt-Decisions.md` M3-PERF-01 (Issue #61, decided by Riegardt):

| Parameter | Value |
|---|---|
| Operation | `GET /api/reports/requests-summary` (filtered + unfiltered) |
| Tool | NBomber 6.7.0 |
| Virtual users | 10 |
| Duration | 2 minutes steady state |
| Data | 1,000 seeded requests |
| Target | p95 < 3s, zero errors |
| Environment | Local app + isolated local PostgreSQL (shared DB untouched) |

### 8.2 Result (TC-P-01, M3-PERF-03 #63)

Run: 2026-10-15, isolated scratch cluster

| Metric | Value |
|---|---|
| Requests | 109,525 |
| Errors | 0 |
| RPS | ~913 |
| p50 | 9.45ms |
| p75 | 14.27ms |
| **p95** | **23.71ms** |
| p99 | 32.9ms |
| Max | 486ms |

**Verdict: PASS** — p95 23.71ms << 3,000ms target, zero errors (~100× headroom)

### 8.3 Limitations

- Production behavior: not proven (different host, volume, network)
- Concurrent-write contention: not tested (read-only workload)
- Beyond 10-user scale: not tested (steady-state measurement)
- Cold-start/JIT warm-up: excluded
- NBomber free license: personal-use-only; flag before organizational use

### 8.4 Reliability

- `/health` (`M3-STG-07`, Issue #81): 200 Healthy with DB connected; 503 with generic message on DB-down, no credential/host leakage
- Rollback/recovery (`M3-STG-09`, Issue #83): Forward-fix policy; rollback requires prior `pg_dump` backup; `Down()` drops `AuditLogs` table (5 rows lost in dry run)

### 8.5 Environment differences (M3-STG-08, Issue #82)

| Area | Dev | Staging (interim) | Staging (compose) | Intended production |
|---|---|---|---|---|
| Hosting | `dotnet run` | Same (`ASPNETCORE_ENVIRONMENT=Staging`) | Containers | Undecided. |
| Database | Local PG18 or shared remote | Separate scratch DB | Compose volume | Remote DB. |
| Data volume | Handful + 1k perf seed | Same | Same | 0-10k records. |
| Swagger | On | On (explicit decision) | On | Off |
| Secrets | `.env` (gitignored) | Env vars | Same via compose | Commited on Deploy server under encryption. |
| TLS | None | None | None | Expected (HSTS) |
| Auth | TestaAuth minting | Same via throwaway minter | Same | JWT Bearer token and bcrypt password hashing. |
| Build identity | Assembly version | `GIT_SHA` env | Sme | Tag-based |

---

## 9. Defects and Quality Interpretation

### 9.1 Canonical findings register

`docs/CI/M3-CI-07-findings.md` (see Section 5.6 for the table). Findings disposition:

| Finding | Interpretation | Status |
|---|---|---|
| FIND-001..004, 006, 007 | Fixed or mitigated for stated scope | Closed for recorded scope; re-verify in CI |
| FIND-005 | No vulnerable packages on 2026-10-14 | Accepted with monitoring by Dependabot (RR-SEC-02) |
| FIND-008 / DEF-005 | Historical credential literal in Git history | Mitigated, open (RR-SEC-01, TD-SEC-01) |
| FIND-009 | Unrelated .gitignore change | Out of scope | Accepted |
| FIND-010 | Hosted GitHub settings not enabled | Mitigated (RR-SEC-03) |

### 9.2 Defect Register

See `docs/CivicConnect_M3_Issues.md` (M3-DEF-01) and source analysis. Key defects:

| ID | Defect | Status |
|---|---|---|
| DEF-001 | Services not registered in DI | Fixed (PR #121/#123) |
| DEF-002 | StatusHistory linked before key exists | Fixed (navigation-based linking) |
| DEF-003 | UpdateState accepts any transition | Fixed (`RequestStateTransitionPolicy`) |
| DEF-004 | Hardcoded identities | Fixed (JWT + role attributes) |
| DEF-005 | DB password in Git history | Mitigated (current value empty for example reasons) |
| DEF-006 | Observer failure after SaveChanges | Fixed (failure isolation in `RequestStateNotifier`) |

### 9.3 Requirement traceability

`docs/RTM-M3.md` is authoritative for requirement wording, acceptance criteria, status vocabulary, and evidence limits. The matrix continues M1/M2 baseline requirements across requester (FR-001–006), staff/security (FR-007–013, NFR-017–019), and management (FR-014–016, NFR-020, NFR-024) slices. Status claims below reflect RTM evidence, not coverage percentages.

| Requirement | Current evidence and disposition |
|---|---|
| FR-001–002 | **Implemented.** Request submission validation, category selection, category-1 strategy and fallback; see `RequesterSliceTests` and `docs/Tests/requester-slice.md`. |
| FR-003 | **Verified** against accepted numeric `Request.Id` reference decision; `CreateRequest_ValidSubmission_ReturnsCreatedDtoAndPersistsInitialHistory`. |
| FR-004–005 | **Verified in test host.** Request ownership, paging/state projection, history chronology/ownership; `RequesterSliceTests.GetMyRequests_*`, `GetHistory_IncludesInitialAndChronologicallyOrdersRows_AndEnforcesOwnership`. |
| FR-006 | **Implemented.** Owner/completion/rating/comment/duplicate constraints; `RequesterSliceTests.Feedback_*`; PostgreSQL unique-index enforcement still depends on relational verification. |
| FR-007 | **Implemented.** Role-gated staff queue and request-detail access have evidence (`GetStaffRequests_Requester_ReturnsForbidden`, `GetRequest_OtherRequester_ReturnsForbidden`, `GetRequest_StaffOutsideAssignment_ReturnsForbidden`), but RTM correctly leaves staff visibility/scope incomplete: queue returns all requests to STAFF/MANAGER and `AccessControlService` is not applied there. |
| FR-008 | **Verified for implemented filters/paging only.** `GetStaffRequests_CombinesFiltersAndPaging`; request list filters state/category/assignee and orders by creation time, but search is not implemented. |
| FR-009 | **Implemented.** Detail endpoint includes category/comments/feedback, ownership checks, and requester internal-comment filtering. `GetRequest_SeededRequestIncludesComment` and requester/staff visibility tests prove selected fields/behavior, not every baseline detail attribute. |
| FR-010 | **Verified for covered endpoint behavior.** Assignment writes assignee/state/history/audit and rejects requester role; `AssignRequest_UpdatesStateHistoryAndAuditWithActor`, `AssignRequest_Requester_ReturnsForbiddenAndLeavesRequestOpen`. |
| FR-011 | **Implemented.** Transition policy and legal/illegal behavior are tested (`RequestTransitionPolicyTests`, `UpdateState_ClosedToOpen_ReturnsBadRequest`, `UpdateState_IllegalTransition_LeavesRequestUnchanged`, reason tests). Full transition/role matrix and exactly-one history/audit verification remain incomplete. |
| FR-012 | **Implemented.** Public/internal comments and visibility are tested (`AddComment_*`, `GetRequest_StaffCanSeeInternalComments`, `GetRequest_RequesterCannotSeeInternalComments`). Dedicated resolution-information assertion remains absent; observer tests prove best-effort notification only. |
| FR-013 | **Implemented.** State transitions to Resolved/Closed exist, and `E2EJourneyTests.SubmitAssignResolve_RequesterSeesUpdate` verifies requester GET returns HTTP 200 and checks expected status-history rows directly in the database; it does not assert the state in the requester response. Journey does not assert response state/history through requester DTO/endpoint, so full requester-visible resolution criterion remains incomplete. |
| NFR-017 | **Implemented; production-like JWT path partly verified.** `JwtAuthFlowTests.RegisterCreateListRead_WithRealJwt_PreservesOwnershipAndRoleRestrictions` exercises registration, issued JWT, ownership, role restriction, and login through JWT bearer. It does not independently validate malformed/expired/wrong-issuer/wrong-audience/signature cases or a real external IdP. |
| NFR-018 | **Implemented; test-host verified.** Role gates and denial/no-side-effect cases cover requester vs staff/manager actions. Test-header authentication proves authorization wiring; JWT flow test covers a real app-issued token for requester flow. |
| NFR-019 | **Implemented.** Requester ownership and internal-comment visibility are evidenced, and staff detail outside-assignment denial is now directly tested by `GetRequest_StaffOutsideAssignment_ReturnsForbidden`. However, `GetStaffRequests` is not assignment-scoped, so the complete staff object-level scope requirement is not met. |
| FR-014–016, NFR-020, NFR-024 | Management RTM records verification for dashboard filters/counts, stalled/reassignment, category administration, audit reads/writes, and stated localhost performance workload. See `DashboardSummaryTests`, `ManagementTests`, `AuditLogTests`, TC-P-01, and `docs/RTM-M3.md` management rows. FR-015/016 acceptance criteria are authored by Riegardt; dashboard age buckets deferred. |

RTM evidence limits remain material: current evidence mixes local automated tests, historical manual HTTP records, test-host checks, and live shared-database checks. `docs/Tests/github_issues_22-25_test_evidence.md` labels several outcomes as recorded/manual and explicitly notes missing state/no-write follow-up evidence. `docs/Tests/issues_48_53_56_test_evidence.md` also describes an earlier 75-test run; neither historical count supersedes the current run below. Full legal transition permutations, requester-visible resolved state assertions, exhaustive detail-field assertions, and production-like JWT negative cases remain gaps. Do not elevate **In Development** rows to **Verified** without new criterion-matched evidence and RTM review.

### 9.4 Quality gates evidence

The checked-in workflow `.github/workflows/test.yml` defines frontend build, .NET restore/build, PostgreSQL migration and schema check, formatting, NuGet vulnerability scan, coverage-script self-test, full test/coverage collection, coverage summary, and TRX/Cobertura artifact upload. Its presence proves configured gates, not that this exact checkout passed hosted CI. No hosted run URL or hosted artifact was verified for this document update.

| Verification | Evidence/result | What it establishes / does not establish |
|---|---|---|
| Full local tests | 2026-10-16, `dotnet test src/CivicConnect.slnx --configuration Release --no-restore --logger "console;verbosity=minimal"`: **118 passed, 0 failed, 0 skipped** | Current local test host result; local factory uses EF Core InMemory unless `ConnectionStrings__DefaultConnection` is supplied. It is not PostgreSQL/hosted CI evidence. |
| Real JWT flow | Included in the 118-test run: `JwtAuthFlowTests.RegisterCreateListRead_WithRealJwt_PreservesOwnershipAndRoleRestrictions` | Exercises app-issued JWT through bearer handler and requester access/role path; does not cover token-validation negative matrix or external IdP. |
| Format/vulnerability/coverage gates | Commands are configured in workflow; earlier local results and self-test are recorded in Section 5.7 / CI evidence docs | Do not claim a fresh local result for this update. Vulnerability findings are scan-time dependent; see `docs/CI/M3-CI-07-findings.md` FIND-005 and RR-SEC-02. |
| PostgreSQL/migrations | CI workflow configures disposable PostgreSQL, applies migrations, confirms `Requests` table; `DatabaseConnectionTests` passed in the 118-test local run | Connectivity to a configured PostgreSQL database passed; this alone does not prove migrations or that all API tests used PostgreSQL (`TestWebApplicationFactory` switches providers only when `ConnectionStrings__DefaultConnection` is set). |
| Hosted CI and GitHub settings | Pending current-run URL/artifacts and live settings evidence | Local checks cannot establish hosted result, required status checks, Dependabot/secret-scanning settings, or repository protection state; see FIND-009, RR-SEC-03. |

Coverage interpretation and critical-path limits are documented in `docs/Milestone 3/PED-Section-5-CI-and-Coverage.md`: line/branch execution is not requirement acceptance evidence. Keep the historical 59/75/93-test records date- and scope-labelled; current local result is 118, not a replacement for the missing hosted run.

### 9.5 Review record

AI assistance and review responsibility are recorded in the role-specific registers: `docs/Ditheto-AI-Usage-Register.md`, `docs/Armand-AI-Usage-Register.md`, and `docs/Riegardt-AI-Usage-Register.md`. Ditheto/Armand registers explicitly leave human review pending for their listed artefacts; Riegardt register records Riegardt's own confirmed reviews for its listed work. These entries are not blanket approval of this consolidated PED or of other owners' requirement mappings.

For this update, RTM evidence was cross-checked against current test names, `RequestsController`, `AccessControlService`, `Program.cs`, workflow configuration, and evidence-folder caveats. This was an AI-assisted repository consistency review, not independent human sign-off. Ditheto/Armand owners must verify their RTM mappings, acceptance criteria, citations, and status changes; release owner must reconcile management claims and dated evidence. Record completed checks with reviewer, date, and exact artefacts in the corresponding AI/review register. Until then, human verification remains pending; hosted-CI and production-readiness claims remain unverified.

---

## 10. Release Readiness

### 10.1 Evidence summary

`docs/Release-Evidence-Summary.md` (M3-REL-01, #98). Nine areas:

| Area | Result | Interpretation |
|---|---|---|
| Build | 0 warnings, 0 errors (analyzer gate) | Compiles under warnings-as-errors |
| CI | Workflow green on phase PRs | Pipeline enforces gates |
| Regression | DEF-006 test; Closed→Open rejection; illegal-transition suite | Fixed defects stay fixed |
| API/integration | All controller tests green (auth, RBAC, transitions, comments, assign, queue) | Endpoints enforce roles, ownership, policy |
| E2E/system | TC-S-01 + TC-S-02 green | Critical journeys work locally and on staging |
| Static/security | Analyzer gate clean; no vulnerable packages | No open findings; .editorconfig exceptions mitigated |
| Performance | TC-P-01: 109,525 reqs, 0 errors, p95 23.71ms | ~100× headroom |
| Defects | DEF-006 verified fixed; pre-existing issues fixed | No known open Riegardt-area defects |
| Acceptance | RTM rows verified | Acceptable per implemented contract |

### 10.2 Readiness classification

**CONDITIONALLY READY** (M3-REL-02, Issue #99, Riegardt judgment call)

**For READY:**
- Clean build under warnings-as-errors
- 118/118 tests including E2E journeys locally and on staging
- Performance PASS with ~100× headroom
- Static/security gates clean
- Audit trail writes verified
- Full evidence indexed

**Against READY (residual conditions):**
- No production host, TLS story, or real IdP (JWT demo scheme)
- Staging is local-simulated
- Dual audit tables unresolved (TD-04)
- FR-015/016 AC self-authored by Riegardt (no M1 baseline)
- No concurrency token (TD-02)
- Production JWT validation not exercised by tests (RR-SEC-04)

### 10.3 Readiness conditions

1. **Demo/assessment use only** via documented paths (local run, Swagger, staging scripts)
2. **No production traffic** until: real IdP, TLS/hosting story, one-table audit decision
3. **Deferred scope stays deferred** (age buckets, outbox, FR-015/016 baseline reconciliation)

### 10.4 Residual risk summary

From `docs/Residual-Risk-Statement.md` (M3-REL-03, #100):

| ID | Risk | Mitigation | Status | Owner |
|---|---|---|---|---|
| RR-SEC-01 | Historical DB password in Git history | Current value empty; CI uses disposable credentials | Resolved | Armand/repository owner |
| RR-SEC-02 | NuGet advisories time-varying | CI rescans every run | Accepted/monitoring | Armand |
| RR-SEC-03 | Hosted GitHub security settings not enabled | Repository evidence recorded; Security settings saved on pdf | Resolved | Armand/repository owner |
| RR-SEC-04 | Test auth doesn't exercise JWT validation | Production JWT config present; documented limitation | Resolved | Armand |
| RR-REL-01 | No production host/TLS/backup | Staging defined; demo only | Open | Riegardt |
| RR-REL-02 | No real identity provider | JWT real where exercised | Resolved | Riegardt |
| RR-REL-03 | Dual audit tables (AuditEntries vs AuditLogs) | One-table team decision pending | Open | Riegardt + Armand |
| RR-REL-04 | Performance claim is localhost-only | Limits documented | Accepted | Riegardt |
| RR-REL-05 | `docker compose up` not executed | Dockerfile/compose reviewed; needs Docker host | Resolved(Gets hosted in CI) | Riegardt |
| RR-REL-06 | FR-015/016 AC self-authored | Traceability recorded | Resolved | Riegardt |

### 10.5 Technical debt

From `docs/Technical-Debt-Register.md` (M3-REL-04, #101):

| ID | Item | Impact | Interest | Status | Owner |
|---|---|---|---|---|---|
| TD-01 | Repository abstraction bypassed | Testability + layering | Grows per endpoint | Open, pending ADR | Ditheto |
| TD-02 | No concurrency token on Request | Last-writer-wins | Grows with staff usage | Open | Armand |
| TD-03 | Observer not outbox-based | Lost notifications | Grows if relied upon | Accepted | Riegardt |
| TD-04 | Dual audit tables | Split evidence | Blocks audit wiring | Open | Riegardt + Armand |
| TD-05 | Dead authorization path | Divergent role strings | Next endpoint may copy | Open | Armand |
| TD-06 | TestAuth crashes on missing JWT_SECRET | Broken local demo flow | Hits new contributors | Open | Armand |
| TD-07 | Audit read capped at Take(200) | Unusable at scale | Grows with audit volume | Accepted | Riegardt |
| TD-08 | Swagger only in Development | Assessors can't explore staging | Blocks demo | Open | Riegardt |
| TD-09 | Dead migration file | DB confusion | Static | Open | Armand |
| TD-10 | Undefined deferred scope | Can't plan | Grows if scope creeps | Open | All |
| TD-11 | UseExceptionHandler points at missing controller | 500s as framework pages | Until Phase 2 PR | Pending merge | Riegardt |

---

## 11. Risk Register

Combined from Ditheto (quality/requirements), Armand (security/process), Riegardt (release/operations) perspectives, per M3-RISK-01..03.

| ID | Risk | Category | Probability | Impact | Mitigation | Status | Owner |
|---|---|---|---|---|---|---|---|
| R1 | CI slips and removes everyone's gate | Process | Medium | High | CI started in Phase 1; quality gate blocks merge | Open | All |
| R2 | Migration snapshot conflicts | Process | Medium | Medium | M3-GOV-04 migration coordination rule | Open | Armand |
| R3 | Auth rework late | Security | Low | High | JWT implemented in Armand slice; test limitations documented (RR-SEC-04) | Open | Armand |
| R4 | Secrets in history | Security | Medium | High | Current config empty; historical credential in Git (DEF-005/FIND-008); rotation pending owner action | Open | Armand/owner |
| R5 | Requirement-implementation drift | Quality | Medium | High | RTM updated in same PRs as tests | Open | Ditheto |
| R6 | Late RTM assembly | Quality | High | Medium | Updated per-test as tests land | Open | Ditheto |
| R7 | Low-value tests counted as padding | Quality | Medium | Medium | Meaningful Test Rule enforced | Open | Ditheto |
| R8 | Staff scope restriction not implemented | Security | High | High | `GetRequest_StaffOutsideAssignment_ReturnsForbidden` tested; `GetRequest_OtherRequester_ReturnsForbidden` tested; assigned staff may still access out-of-scope requests — documented gap | Open | Armand |
| R9 | Production JWT not exercised | Security | High | High | Test handler proves wiring only (RR-SEC-04); production JWT validation not tested | Open | Armand |
| R10 | No production hosting/TLS/IdP | Release | High | High | Conditional READY; demo/assessment only | Open | Riegardt |
| R11 | Dual audit tables unresolved | Release | Low | Medium | One-table decision pending (TD-04/RR-REL-03) | Open | Riegardt |
| R12 | Performance claim is localhost-only | Release | Medium | Medium | Limits documented in Riegardt-Decisions.md | Accepted | Riegardt |

---

## 12. Evidence Index

### 12.1 Core registers

| Register | File | Owner |
|---|---|---|
| Requirements Traceability Matrix | `docs/RTM-M3.md` | Ditheto (continues with Armand/Riegardt rows) |
| Defect Register | see `docs/CivicConnect_M3_Issues.md` (DEF-nnn) | Ditheto |
| Technical Debt Register | `docs/Technical-Debt-Register.md` | Riegardt |
| Residual Risk Statement | `docs/Residual-Risk-Statement.md` | Riegardt |
| Static/security findings | `docs/CI/M3-CI-07-findings.md` | Armand |
| AI Usage Register (Ditheto) | `docs/P1-AI-Usage-Register.md` | Ditheto |
| AI Usage Register (Armand) | `docs/P2-AI-Usage-Register.md` | Armand |
| AI Usage Register (Riegardt) | `docs/P3-AI-Usage-Register.md` | Riegardt |
| Riegardt Decisions log | `docs/P3-Decisions.md` | Riegardt |
| Release Evidence Summary | `docs/Release-Evidence-Summary.md` | Riegardt |

### 12.2 Test evidence

| File | Trace |
|---|---|
| `docs/Tests/requester-slice.md` | FR-001–006, TC-U-05/06 |
| `docs/Tests/github_issues_22-25_test_evidence.md` | FR-007–013, TC-22..25 |
| `docs/Tests/issues_48_53_56_test_evidence.md` | TC-U-03..04, TC-I-01..05, TC-N-01..03 |
| `docs/Tests/m3-test-05-evidence.md` | TC-U-05, TC-U-06 |
| `docs/Tests/issue_27_observer_test_evidence.md` | FR-012, DEF-006, TC-I-04 |
| `docs/Postman_Request_Comments_Test_Cases.md` | TC-01..14 (manual HTTP) |

### 12.3 Source evidence

| Artefact | Path |
|---|---|
| Solution file | `src/CivicConnect.slnx` |
| Controllers | `src/CivicConnect.Web/Controllers/` |
| DbContext + migrations | `src/CivicConnect.Web/Data/`, `src/CivicConnect.Web/Migrations/` |
| DB schema script | `src/CivicConnect.Web/db_schema.sql` |
| State transition policy | `src/CivicConnect.Application/Policies/RequestStateTransitionPolicy.cs` |
| Observer pattern | `src/CivicConnect.Application/Patterns/Observer/` |
| Strategy pattern | `src/CivicConnect.Application/Patterns/Strategy/` |
| CI workflow | `.github/workflows/test.yml` |
| Dockerfile | `Dockerfile` |
| Staging compose | `docker-compose.staging.yml` |
| Vulnerability gate script | `scripts/check-vulnerabilities.py` |
| Coverage script | `scripts/coverage-summary.py` |
| Staging E2E script | `scripts/staging-e2e.ps1` |
| Staging smoke script | `scripts/staging-smoke.ps1` |
| Performance harness | `src/CivicConnect.Perf/` |
| Perf seed | `src/CivicConnect.Perf/seed-perf.sql` |

### 12.4 PED internal cross-references

- Section 2 ←→ Section 3 (conformance deviations → controlled changes)
- Section 5 ←→ Section 7 (CI evidence → test results)
- Section 5 ←→ Section 9 (CI findings register → defect discharge)
- Section 6 ←→ RTM (`docs/RTM-M3.md`) (risk-based selection trace)
- Section 8 ←→ Section 9 (perf limits → interpretation)
- Section 8 ←→ Section 10 (perf evidence → readiness)
- Section 10 ←→ Residual Risk Statement (RR-REL-01..06)
- Section 10 ←→ Technical Debt Register (TD-01..TD-11)
- Section 10 ←→ Release Evidence Summary (M3-REL-01)

---

## 13. Harvard References and Citations

1. Microsoft. (2026). *ASP.NET Core Documentation*. https://learn.microsoft.com/aspnet/core
2. Microsoft. (2026). *Entity Framework Core Documentation*. https://learn.microsoft.com/ef/core
3. Microsoft. (2026). *.NET 10 SDK release notes*. https://learn.microsoft.com/dotnet/core/whats-new
4. OWASP Foundation. (2024). *OWASP Top 10*. https://owasp.org/www-project-top-ten
5. OWASP Foundation. (2024). *OWASP API Security Top 10*. https://owasp.org/www-project-api-security
6. ISO/IEC. (2017). *ISO/IEC 25010:2011 Software engineering — Product quality requirements and evaluation*.
7. IEEE. (2017). *IEEE Std 829-2008: Standard for Software and System Test Documentation*.
8. Fowler, M. (2003). *Patterns of Enterprise Application Architecture*. Addison-Wesley.
9. Gamma, E., Helm, R., Johnson, R., Vlissides, J. (1995). *Design Patterns*. Addison-Wesley.
10. Microsoft. (2026). *Coverlet — .NET code coverage*. https://github.com/coverlet-io/coverlet
11. Microsoft. (2026). *NBomber — .NET load testing framework*. https://nbomber.com
12. SEN381 Course Team. (2026). *Milestone 1 Brief*. Repository root.
13. SEN381 Course Team. (2026). *Milestone 2 Brief + PED v2.0*. `docs/Milestone 2/`

---

## Appendix A: M2 Baseline Version History

| Version | Date | Changes |
|---|---|---|
| PED v1.0 | M1 | Initial baseline (PED v1.0) — milestone brief |
| PED v2.0 | M2 | Full PED with FR-001–016, NFR-017–020, NFR-024; layered architecture; Observer/Strategy patterns; REST API; PostgreSQL/EF Core; React SPA baseline |
| PED v3.0 | M3 | Continued from v2.0 with M3 implementation evidence, controlled changes, CI gates, test results, staging, performance, and release readiness |

---

## Appendix B: Controlled Changes Log

| Change | Original | Revised | Evidence | Status |
|---|---|---|---|---|
| Repository abstraction | Repository interfaces | DbContext-as-UoW accepted | TD-01 | Accepted |
| API versioning | `/api/v1/` | `/api` | RTM evidence limits | Accepted |
| State pattern | State objects | Central transition table | `RequestStateTransitionPolicy.cs` | Accepted |
| Auth mechanism | Identity provider TBD | JWT bearer + test handler| `Program.cs`, RR-SEC-04 | Accepted with risk |
| Frontend | React 19 SPA | React 19 SPA (merged PR #134) | `ClientApp/`, Dockerfile | Conformant |
| Notification | Synchronous observer | Log-only, best-effort (observer kept) | `RequestStateNotifier.cs`, TD-03 | Accepted |
| Concurrency | None specified | None (no token) | TD-02 | Open |

---