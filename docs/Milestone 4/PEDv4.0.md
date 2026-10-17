# CivicConnect — Project Engineering Document v4.0 Final

**SEN381 Software Engineering 381 — Milestone 4**
**Purpose:** Finalise, consolidate and conclude the CivicConnect project
**PED status:** Final closure document; continuation of PED v3.0, not a replacement history

> This document reports repository evidence available while preparing final closure. It does not claim that an unperformed test, deployment, human review, or team sign-off occurred. Fields marked **PENDING** require completion before formal submission.

## Final submission information

| Field | Final value |
|---|---|
| Team | CivicConnect / SEN381 team |
| Members | Armand Johannes Vermeulen (602636); Ditheto Lesiba Phaho (601133); Riegardt Christopher Weich (602547) |
| Repository | https://github.com/Armand-JV/SEN381-Project |
| Repository baseline inspected | `70a36fb9ad99e1a51bcaab43741d2845c8d04833` (`70a36fb`, 2026-10-17) |
| Assessed commit/tag | **PENDING team freeze; no Git tag exists in inspected repository** |
| PED | v4.0 Final |
| Final RTM | `docs/Milestone 3/Individual M3 documents/RTM-M3.md` (continued, not recreated) |
| Final deployment declaration | **PENDING team approval; evidence supports staging / assessment-only no-go for production** |
| Submission date | **PENDING** |

## 1. M4 closure statement

M1 defined a controlled service-request platform for requesters, staff, management and administrators. M2 selected a .NET/ASP.NET Core, React and PostgreSQL design with lifecycle, authorisation, audit and maintainability drivers. M3 constructed and verified a substantial integrated application, including requester, staff, management, authentication, audit, staging and performance slices.

M4 adds no new major feature programme. It closes the loop from original commitment to final outcome. The final product is a React 19/Vite SPA served by ASP.NET Core 10, backed by PostgreSQL through EF Core/Npgsql. It supports request creation, requester views and history, feedback, staff queue and workflow operations, comments, management reporting, stalled-request oversight, category administration, authentication, role checks, audit reads and health/version endpoints.

The evidence supports a coherent assessment/demo product. It does not support production release: no production host/TLS evidence, real identity provider, verified backup regime, or verified Docker Compose execution is recorded. The recommended state is **staging under a justified production no-go** until release blockers are addressed; team approval remains pending.

## 2. M1 commitment and final scope accounting

### 2.1 Status rules

- **IMPLEMENTED / Met:** acceptance criterion supported by final implementation and matching evidence.
- **PARTIALLY IMPLEMENTED / Partially Met:** useful behaviour exists, but an original condition, scope boundary or evidence requirement remains incomplete.
- **NOT IMPLEMENTED / Not Met:** no evidence supports the original commitment.
- **CHANGED/REMOVED THROUGH APPROVED CHANGE:** only used where a traceable approved change altered the baseline. A later feature does not silently replace an M1 commitment.
- Completion calculation: Met = 1.0; Partially Met = 0.5; Not Met = 0.0.

### 2.2 Original M1 in-scope acceptance criteria

The table preserves M1's original meaning. `AC-0010` is shown as corrected `AC-010` under M2 change request CR-003. Later FR numbering and implementation labels do not rewrite these commitments.

| M1 ID | Original commitment / acceptance criterion | Final status | Evidence | Variance and consequence |
|---|---|---|---|---|
| AC-001 | Valid authorised submission creates a request; missing required information is rejected. | **IMPLEMENTED / Met** | `RequestsController.CreateRequest`; `RequesterSliceTests.CreateRequest_InvalidInput_ReturnsFieldValidationAndPersistsNothing`; `RequestControllerTests.CreateRequest_*` | Title, description and category validation exists; API evidence is stronger than original baseline wording. |
| AC-002 | Requester selects a controlled category; submission without/with invalid category is denied. | **IMPLEMENTED / Met** | `GET /api/categories`; `CategoryValidationContext`; `Category1ValidationStrategy`; category tests and requester-slice tests | Category list and server-side validation exist. |
| AC-003 | Each successful submission receives a unique identifier usable for later operations. | **IMPLEMENTED / Met** | Numeric `Request.Id`; `CreateRequest_ValidSubmission_ReturnsCreatedDtoAndPersistsInitialHistory`; requester UI/API reference | Team accepted numeric ID as request reference; no separate formatted reference number. |
| AC-004 | Authorised requester views submitted request and current status. | **IMPLEMENTED / Met** | `/api/requests/mine`; ownership tests; `JwtAuthFlowTests.RegisterCreateListRead_WithRealJwt_PreservesOwnershipAndRoleRestrictions` | Production token-negative matrix remains unverified, but ownership/status behaviour is evidenced in the test host and real app-issued JWT flow. |
| AC-005 | Requester views submitted request history. | **IMPLEMENTED / Met** | `/api/requests/{id}/history`; `RequesterSliceTests.GetHistory_IncludesInitialAndChronologicallyOrdersRows_AndEnforcesOwnership` | History includes initial entry, ordering, reason and ownership enforcement. |
| AC-006 | System gives meaningful feedback/notification when request is accepted, rejected, updated or fulfilled. | **NOT IMPLEMENTED / Not Met** | `RequestStateNotifier`; `docs/Tests/issue_27_observer_test_evidence.md`; React request detail/history views | No durable or requester-facing notification feature is evidenced. Status/history views support AC-004/005, not this separate notification commitment; log-only observer is not requester feedback (TD-03). |
| AC-007 | Staff views only service requests relevant to their role and permissions; outside-authority requests unavailable. | **PARTIALLY IMPLEMENTED / Partially Met** | Staff role endpoint and `GetRequest_StaffOutsideAssignment_ReturnsForbidden`; `RequestControllerTests` | Detail scope is tested, but staff queue returns broad staff-visible results rather than consistently enforcing assigned-only scope. |
| AC-008 | Staff filters, searches and/or sorts using request attributes; results match selected attribute. | **IMPLEMENTED / Met** | `GetStaffRequests_CombinesFiltersAndPaging`; state/category/assignee filters, paging and creation ordering | The M1 criterion permits at least one attribute; implemented filters meet that minimum. Full-text search remains deferred (TD-10). |
| AC-009 | Staff views full request details and attributes. | **PARTIALLY IMPLEMENTED / Partially Met** | `RequestsController.GetRequest`; `GetRequest_SeededRequestIncludesComment`; staff/internal-comment tests | Detail endpoint works, but no dedicated assertion proves every baseline attribute; staff-scope gap remains. |
| AC-010 | Unauthorised staff cannot change assignment; authorised assignment is recorded. | **IMPLEMENTED / Met** | `AssignRequest_UpdatesStateHistoryAndAuditWithActor`; requester-forbidden assignment test; `ManagementTests` | Assignment, actor, state/history and audit behaviour are evidenced for covered paths. |
| AC-011 | Authorised staff update requests through controlled processes; only authorised transitions recorded. | **PARTIALLY IMPLEMENTED / Partially Met** | `RequestStateTransitionPolicy`; illegal-transition/reason/history tests; `RequestControllerTests` | Core policy works, but complete legal/rejected permutation and full role matrix from M2 plan are not evidenced. Implementation changed from State objects to central transition table; recorded in PED v3. |
| AC-012 | Staff track actions, comments and resolution information. | **PARTIALLY IMPLEMENTED / Partially Met** | `AddComment_*`; internal/public visibility tests; `Comment` entity; state/history endpoints | Comments and visibility work; dedicated exhaustive resolution-information evidence is absent. |
| AC-013 | Authorised staff resolve/close requests; requester sees the change. | **IMPLEMENTED / Met** | `UpdateState_ValidTransition_ReturnsOkAndPersistsHistory`; `E2EJourneyTests.SubmitAssignResolve_RequesterSeesUpdate`; requester detail endpoint | E2E evidence confirms requester retrieval after resolution. The test asserts history at DB level; that is an evidence-depth limit, not absence of required requester visibility. |
| AC-014 | Management views useful service activity information, including major changes such as ownership change. | **IMPLEMENTED / Met** | `ReportsController`; `ManagementTests`; `AuditLogTests`; `GET /api/audit`; dashboard tests | Dashboard and audit provide activity/ownership information. Audit table duplication remains a quality limitation, not hidden. |
| AC-015 | Management identifies open, overdue, closed and resolved requests. | **PARTIALLY IMPLEMENTED / Partially Met** | `GET /api/reports/requests-summary`; stalled management tests; `docs/Milestone 3/Individual M3 documents/RTM-M3.md` | Open/state counts and stalled age filtering exist. M1's overdue rule/due-date semantics are undefined and not implemented; age buckets remain deferred. |
| AC-016 | Management views requests by category, status and/or relevant criteria. | **IMPLEMENTED / Met** | Dashboard filters and `DashboardSummaryTests`; management/API evidence | Category/state/assignee/date filters provide the required attribute view. Category administration is additional functionality, not the basis of this status. |
| AC-017 | Users are authorised before protected functionality/request information is provided. | **IMPLEMENTED / Met** | JWT bearer configuration; `[Authorize]`; anonymous 401 tests; `JwtAuthFlowTests` | Controller wiring and app-issued JWT happy path are evidenced. External IdP and malformed/expired/wrong-issuer token cases remain untested. |
| AC-018 | Role-based authorisation limits actions to allocated roles. | **IMPLEMENTED / Met** | Role attributes; requester-forbidden staff/assignment tests; auth service/JWT tests | Wrong-role denial and no-side-effect cases exist; production configuration coverage remains limited. |
| AC-019 | Sensitive request information is available only to relevant authorised staff. | **PARTIALLY IMPLEMENTED / Partially Met** | Ownership tests; requester internal-comment filtering; staff scope test | Requester isolation and internal-comment hiding work, but staff queue object scope is not consistently assigned-only. |
| AC-020 | Major request changes are tracked; assignment/status changes record responsible actor. | **IMPLEMENTED / Met** | `AuditLogTests`; `ManagementTests`; `GET /api/audit`; `AuditLog` migration | Consequential management paths record audit data. P2 assignment also writes legacy `AuditEntries`; one-table cleanup remains open. |
| AC-021 | Clear labels, validation and feedback support submission and subsequent changes. | **PARTIALLY IMPLEMENTED / Partially Met** | API problem details, validation tests, React SPA pages and built assets | API feedback is evidenced; no complete usability evaluation against every original UI criterion is recorded. |
| AC-022 | Request information persists and remains retrievable with its ID after submission. | **IMPLEMENTED / Met** | EF/PostgreSQL migrations; initial history tests; `DatabaseConnectionTests`; CI migration workflow | Relational persistence and migration evidence exist; recovery/backup operational regime remains a release blocker. |
| AC-023 | Modular components have defined responsibilities and support isolated change. | **PARTIALLY IMPLEMENTED / Partially Met** | Domain/Application/Web projects; patterns; CI/build; `README.md` | Layering and patterns exist, but direct `DbContext` use in controller and unused repository interfaces remain TD-01. |
| AC-024 | At least 95% of regular operations provide feedback within three seconds under defined load. | **IMPLEMENTED / Met within stated limits** | TC-P-01: 109,525 requests, 0 errors, p95 23.71 ms; `src/CivicConnect.Perf/`; `docs/Milestone 3/Individual M3 documents/P3-Decisions.md` | Strong localhost scratch-DB result. It is not production or broad load-mix evidence; target interpretation follows M2 CR-001. |

### 2.3 Scope calculation

Applicable approved M1 acceptance criteria: **24**. No approved M4 baseline change is evidenced; therefore denominator remains 24.

| Result | Count | Value |
|---|---:|---:|
| Met | 14 | 14.0 |
| Partially Met | 9 | 4.5 |
| Not Met | 1 | 0.0 |
| **Total** | **24** | **18.5** |

**Scope Completion Percentage = (18.5 ÷ 24) × 100 = 77.08% (77.1%).**

AC-006 remains in the denominator as a failed in-scope commitment. M1's future-scope SMS/email gateways are excluded; the in-scope generic status-feedback/notification criterion is not.

This is **2.9 percentage points below** the M4 target of 80%. The result is not inflated by treating changed or additional M3 features as replacements for M1 commitments. The largest stakeholder consequences are incomplete durable notification, incomplete staff object scope, incomplete overdue semantics, incomplete exhaustive detail evidence, and incomplete production assurance. No critical requirement is silently claimed complete because of the percentage.

### 2.4 Deferred and out-of-scope work

These items are not failed M1 commitments:

- **Out of scope:** native mobile applications, real-time chat, payment/billing, external municipal integrations, multi-language localisation, AI categorisation.
- **Deferred/future:** SLA/overdue escalation and mature age buckets, SMS/email notification, public aggregate transparency, predictive/advanced analytics, durable notification outbox/retry, production identity provider/hosting/TLS, horizontal scaling and other items recorded in the M2/M3 registers.

## 3. Final product and integration

### 3.1 Final assessed product

The integrated product consists of:

- React 19 + TypeScript/Vite frontend under `src/CivicConnect.Web/ClientApp/`, built into `wwwroot` and served by ASP.NET Core.
- .NET 10 solution at `src/CivicConnect.slnx`, with Domain, Application, Web, Tests and performance projects.
- PostgreSQL/EF Core persistence, migrations and health endpoint.
- JWT credential authentication, Requester/Staff/Manager/Admin role paths, manager bootstrap and privileged provisioning.
- Requester, staff and management workflows documented in `README.md` and implemented under `src/CivicConnect.Web/Controllers/`.

Critical demonstrable journeys:

1. Register/login or use controlled demo identity → requester submits validated request → requester views own request and history.
2. Staff views queue → assigns request → applies legal state transitions → adds public/internal comment → requester sees permitted result.
3. Manager views filtered dashboard/audit → identifies stalled work → reassigns request → manages categories.
4. Resolved/closed requester submits one feedback item; management dashboard remains available.

### 3.2 Finalisation versus new development

The last recorded work integrated credential authentication and manager provisioning, updated release evidence, and organised M3 artefacts. M4 closure work is documentation, evidence reconciliation, scope calculation, release decision and conclusion. No new major M4 feature is claimed here.

## 4. Final acceptance and verification evidence

M3 recorded a local Release test run of **118 passed, 0 failed, 0 skipped on 2026-10-16**. This is historical evidence for the recorded M3 checkout; it is not a fresh test result for the final assessed baseline in this document. The workflow configuration exists at `.github/workflows/test.yml` and includes frontend build, .NET build, PostgreSQL migration, formatting, vulnerability checks, tests, coverage and artifacts. A hosted run URL/artifact matching the final baseline is **not confirmed** here.

| Evidence | Final interpretation | Limitation |
|---|---|---|
| `RequesterSliceTests`, `RequestControllerTests` | Requester/staff API behaviour and negative access cases | Test host usually uses EF InMemory unless connection string supplied; criterion-specific gaps remain. |
| `JwtAuthFlowTests`, `AuthServiceTests`, `PrivilegedProvisioningTests`, `ManagerBootstrapTests` | Credential/JWT happy path and manager provisioning | No full malformed/expired/wrong issuer/audience/signature matrix or external IdP. |
| `E2EJourneyTests` | Two integrated application journeys | One journey's requester history/state assertion is DB-level rather than exhaustive endpoint-level evidence. |
| `DashboardSummaryTests`, `ManagementTests`, `AuditLogTests` | Management counts, stalled/reassignment, category and audit behaviours | Staging/production equivalence not proven. |
| `src/CivicConnect.Perf/` and TC-P-01 | Local performance target passed with large headroom | Localhost, read-heavy, 10-VU, 1,000-row workload only. |
| `.github/workflows/test.yml` | Reproducible configured quality gates | Configuration proves intended gates, not a green final hosted run. |
| `scripts/staging-e2e.ps1`, `scripts/staging-smoke.ps1`, `/api/version`, `/health` | Staging/demo verification path | Docker Compose execution and production deployment not verified. |

No new M4 test-count target is introduced. Any final change must trigger targeted re-verification before team submission; the team must record that result in the existing RTM and registers.

## 5. M3 issue, defect, risk and debt closure

### 5.1 Important fixes evidenced

The M3 defect record reports fixes for DI registration, initial history linking, illegal state transitions, hardcoded identities, observer failure isolation, test isolation and related harness issues. Supporting evidence includes `RequestStateTransitionPolicy`, controller tests, observer tests, JWT flow tests and the canonical defect/findings records.

### 5.2 Items still open or requiring explicit owner confirmation

The following remain visible; PED v4 does not silently close them:

- **TD-01:** direct `DbContext` use and unimplemented repository interfaces.
- **TD-02:** no optimistic concurrency token; concurrent edits can be last-writer-wins.
- **TD-03:** observer delivery is log-only/best-effort; no outbox/retry.
- **TD-04:** `AuditEntries` and `AuditLogs` coexist.
- **TD-05–TD-11:** dead authorisation path, missing-secret test-auth failure, capped audit reads, Swagger/staging decision, dead migration, deferred-scope definition and exception-handler/documentation items as recorded in the canonical register.
- **RR-SEC-01/02/03/04:** historical credential exposure, time-varying advisories, hosted GitHub security settings, and incomplete production JWT validation.
- **RR-REL-01/02/03/05/06:** no production host/TLS/backup regime, no real IdP, dual audit tables, unexecuted Compose path, and authored FR-015/016 criteria.
- Staff queue scope, overdue semantics, complete transition permutations, exhaustive detail/resolution assertions and final hosted CI evidence remain acceptance limitations.

Canonical records: `docs/Milestone 3/Individual M3 documents/Technical-Debt-Register.md`, `Residual-Risk-Statement.md`, `Release-Evidence-Summary.md`, `docs/CI/M3-CI-07-findings.md` and `docs/Github/CivicConnect_M3_Issues.md`.

### 5.3 Closure conclusion

Important M3 defects were addressed sufficiently for an integrated assessment/demo candidate. Remaining debt and release risks are material for production but controlled and disclosed for a staging/no-go final decision. Human review of AI-assisted artefacts and final owner confirmation remain pending where the registers say pending.

## 6. Controlled decisions and consequences

| Decision | Final consequence |
|---|---|
| Modular .NET monolith rather than microservices | Proportionate deployment and transactional workflows for a three-person team; module discipline and whole-application scaling remain limitations. |
| Direct EF `DbContext` unit-of-work in controller | Shorter delivery path and effective integration tests; weaker application-layer isolation and growing TD-01. |
| Central transition table rather than planned State objects | Simpler, testable lifecycle rules; M2 State-pattern conformance changed and full transition matrix still needs evidence. |
| JWT bearer plus test authentication handler | Real app-issued JWT flow and role gates exist; test handler does not prove production identity-provider validation. |
| Synchronous best-effort observer | Low infrastructure cost and failure isolation; no durable requester-notification guarantee, hence TD-03 and unmet AC-006. |
| Staging no-go for production | Protects personal information, integrity and operational reliability; production value is deferred until host, TLS, IdP, backup and final verification evidence exist. |

## 7. Deployment and go-live decision

**Recommended final state: staging; no-go for production.** Team approval remains pending.

Evidence supports local/interim staging, environment-based secrets, PostgreSQL migrations, health/version endpoints, Dockerfile/Compose definitions and a staging E2E script. Evidence does not establish a production host, TLS certificate story, real identity provider, tested backup/restore regime for the release, or executed Compose environment. Releasing under those conditions would expose authentication, privacy, integrity and operational risks recorded as RR-SEC-04 and RR-REL-01/02/05.

Before production approval, the team must: select and evidence hosting/TLS; use a real managed identity provider or equivalent validated production identity path; verify controlled secrets and rotation; execute migration and rollback/backup procedures; run final-baseline smoke/E2E/security checks; reconcile the two audit stores; and capture hosted CI/deployment evidence. This is an evidence-based no-go, not a claim that the product is unusable.

Team's final declaration and owner sign-off: **PENDING**.

## 8. Final RTM and artefact continuity

The existing RTM remains authoritative: `docs/Milestone 3/Individual M3 documents/RTM-M3.md`. It retains M1/M2 wording and M3 evidence. Its final continuation must point to this PED's AC-001–AC-024 table and preserve evidence limitations. In particular, FR-006/FR-014/FR-015/FR-016 later labels must not erase the original M1 meanings. The RTM must be updated by the team after any final fixes, with the exact assessed commit and human reviewer/date recorded.

Existing registers continue rather than being recreated:

- Requirements: `docs/Milestone 3/Individual M3 documents/RTM-M3.md`
- Defects/issues: `docs/Github/CivicConnect_M3_Issues.md`
- Technical debt: `docs/Milestone 3/Individual M3 documents/Technical-Debt-Register.md`
- Residual risk: `docs/Milestone 3/Individual M3 documents/Residual-Risk-Statement.md`
- CI/security findings: `docs/CI/M3-CI-07-findings.md`
- AI use: `docs/AI Use Registers/P1-AI-Usage-Register.md`, `P2-AI-Usage-Register.md`, `P3-AI-Usage-Register.md`

## 9. Final reflection and conclusion

**Original problem/value.** CivicConnect replaces fragmented email, phone, messaging, spreadsheet and paper handling with a controlled request record, status lifecycle, ownership, history, management visibility and access controls. The final product meaningfully addresses the core problem for assessment/demo use.

**Scope.** 18.5/24 applicable M1 acceptance-criterion points equals 77.1%. Core submission, categorisation, identity, requester history, assignment, management filtering, persistence, audit and performance work. Requester-facing status notification (AC-006) is not met; complete staff scope, overdue semantics, exhaustive evidence and production assurance remain incomplete or partial.

**Quality.** Confidence comes from automated unit/API/integration/E2E evidence, PostgreSQL migration checks, configured CI gates, static/dependency checks and the local performance run. Confidence is limited by the historical date of the 118-test run, missing matching hosted-CI evidence, mixed InMemory/PostgreSQL test providers, incomplete negative JWT validation, and localhost-only performance evidence.

**Risk/debt.** No material limitation is hidden. Production security/operations blockers remain open; technical debt affects maintainability, concurrency, notifications and audit clarity.

**Execution.** The team prioritised core slices and stabilisation over mobile, external integration, AI categorisation, SMS/email and advanced analytics. Late integration and evidence reconciliation created drift between M1 meanings and M3 feature labels; this PED exposes rather than conceals that drift.

**Next work.** Freeze a release tag; rerun critical tests and hosted CI; complete staff queue scope and requester resolution assertions; define overdue semantics; consolidate audit tables; add concurrency protection and durable notifications; validate production JWT/IdP, TLS, backup/recovery and deployment.

**Conclusion.** CivicConnect is a substantial, coherent staging/demo product that delivers most core request-management value but does not meet the 80% scope target on the strict original M1 acceptance baseline and is not production-ready. Its final outcome is credible because gaps, evidence limits, risks and consequences are explicit. The project is concluded only when the team freezes the exact commit, updates the RTM/register owner statuses, records final verification and signs this PED.

## 10. Final demonstration script

1. Identify the exact frozen commit/tag and show `/api/version`.
2. Demonstrate requester registration/login, valid submission, validation failure, own request and history.
3. Demonstrate staff queue filters, assignment, legal transition, comment visibility and requester-facing result.
4. Demonstrate manager dashboard, stalled request, reassignment, category operation and audit trail.
5. Use the M1 scope table to show one met, one partial, one deferred/out-of-scope item and reproduce 77.1% arithmetic.
6. Show selected RTM evidence, M3 defect/debt/risk status and performance result with limits.
7. Defend staging/no-go decision, blockers and next remediation.
8. Each member traces one contribution from M1 requirement through implementation, evidence, limitation and consequence.

## 11. Individual engineering defence preparation

| Member | Evidence to defend |
|---|---|
| Armand Vermeulen | Staff/security construction, JWT and role controls, CI/workflow, credential provisioning, technical debt and deployment security limits. Trace AC-017–AC-020 to `Program.cs`, controllers, auth tests and CI. |
| Ditheto Phaho | M1 requirements baseline, requester slice FR-001–006, validation, ownership/history/feedback, RTM and defect interpretation. Explain AC-006 notification variance and numeric-ID decision. |
| Riegardt Weich | Management dashboard/oversight/category/audit slice, staging/performance/release evidence and no-go reasoning. Trace AC-014–AC-016 and AC-024 to tests, performance harness and risk register. |

All members must understand combined product, final scope arithmetic, controlled changes, unresolved risks, AI accountability and evidence limitations—not only their own code.

## 12. Final evidence index

| Evidence area | Controlled location |
|---|---|
| M1 baseline | `docs/Milestone 4/SEN381 Milestone 1.md` |
| M2 design baseline | `docs/Milestone 4/CivicConnect_PED_v2.md` |
| M3 historical PED | `docs/Milestone 4/PEDv3.0.md`; `docs/Milestone 3/PEDv3.0.md` |
| Final requirements trace | `docs/Milestone 3/Individual M3 documents/RTM-M3.md` |
| Source | `src/` |
| Tests | `src/CivicConnect.Tests/` |
| Test evidence | `docs/Tests/` |
| CI | `.github/workflows/test.yml` |
| Staging | `Dockerfile`, `docker-compose.staging.yml`, `scripts/staging-e2e.ps1`, `scripts/staging-smoke.ps1` |
| Performance | `src/CivicConnect.Perf/`, `docs/Milestone 3/Individual M3 documents/P3-Decisions.md` |
| Defects | `docs/Github/CivicConnect_M3_Issues.md` |
| Debt and risks | `docs/Milestone 3/Individual M3 documents/Technical-Debt-Register.md`; `Residual-Risk-Statement.md` |
| CI/security findings | `docs/CI/M3-CI-07-findings.md` |
| AI registers | `docs/AI Use Registers/` |
| Product operation | `README.md` |

## 13. Version history

| Version | Date | Author | Change |
|---|---|---|---|
| PED v1.0 | 2026-09-09 | Team | M1 problem, stakeholders, scope, requirements and governance baseline. |
| PED v2.0 | 2026-09-30 | Team | M2 architecture, technology, data, ADRs, interface and RTM baseline. |
| PED v3.0 | 2026-10-16 | Team | M3 construction, tests, CI, staging, performance, defects and release-candidate evidence. |
| PED v4.0 | 2026-10-17 | Team | Final scope accounting, evidence reconciliation, M3 closure, deployment decision and project conclusion. |

## References

Use the Harvard reference list retained in PED v3.0 and the M1/M2 source sections. Primary project evidence is the repository, its controlled history, RTM and registers listed above. External sources include ASP.NET Core, EF Core, PostgreSQL, OWASP, ISO/IEC 25010, IEEE testing guidance, Fowler, Gamma et al., NIST/ANSI RBAC, POPIA, NBomber and the SEN381 milestone briefs.
