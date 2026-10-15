# CivicConnect M3: GitHub Issues List

Generated from the M1/M2/M3 briefs, the M3 work split and the repository snapshot. **Read the assumptions first.**

## Assumptions to confirm before importing

1. **PED v2.0 was not in the project files**, and the M1 brief contains no requirement IDs. I used the IDs that do appear in the work split (FR-001..016, NFR-017..020, NFR-024) and a consistent scheme for everything else: `FR-nnn`, `NFR-nnn`, `DEF-nnn` (Defect Register), `ADR-M3-nn`, `M3-<AREA>-nn` (e.g. `M3-CI-03`), test cases `TC-U/B/I/S/N/P-nn`. If PED v2.0 uses different prefixes, find-and-replace them in `create_m3_issues.sh` before running.
2. **FR/NFR titles are inferred.** FR-001..006 follow the work split's order (submit, categorise, unique ID, status, history, feedback). FR-007..016 and NFR-019 are marked *working title* because I cannot see their wording. Verify each against the RTM.
3. **DEF-nnn issues come from reading the code, not from running it.** DEF-002 is explicitly *suspected*. Reproduce each one (ideally with a failing test) before logging it as confirmed.
4. Owners follow the work split (P1 Quality, P2 Construction/CI/Security, P3 Release/Ops). Put GitHub usernames at the top of the script to auto-assign.
5. Dates are not set; fill them from the Moodle deadline (M3-PLAN-02).

## Summary (105 issues)

Per owner: P1: 30, P2: 36, P3: 33, all: 6

| ID | Title | Owner | Type | Phase | Priority |
|---|---|---|---|---|---|
| M3-PLAN-01 | Freeze M3 core scope and release-candidate boundary | all | process | 1 | high |
| M3-PLAN-02 | Create GitHub Project board and fill phase dates against Moodle deadline | all | process | 1 | medium |
| M3-CONF-01 | Build M2-to-implementation conformance table | P1 | docs | 1 | high |
| M3-CONF-02 | ADR-M3-01: Repository abstraction vs direct DbContext use | P1 | adr | 1 | medium |
| M3-CONF-03 | ADR-M3-02: Authentication and authorisation approach | P1 | adr | 1 | high |
| M3-CONF-04 | ADR-M3-03: Request state-transition model and permitted-transition table | P1 | adr | 1 | high |
| FR-001 | Submit service request (validated, authenticated, DTO response) | P1 | feature | 2 | high |
| FR-002 | Category selection and category-specific validation strategies | P1 | feature | 2 | high |
| FR-003 | Unique, human-readable request reference ID | P1 | feature | 2 | medium |
| FR-004 | Requester views own request and status | P1 | feature | 2 | high |
| FR-005 | Requester views status history timeline | P1 | feature | 2 | medium |
| FR-006 | Requester feedback on a completed request | P1 | feature | 3 | medium |
| M3-UI-01 | Decide and build minimal UI (or documented API-only demo path) for E2E journeys | P3 | feature | 2 | high |
| FR-007 | Staff request queue with filters (working title) | P2 | feature | 2 | high |
| FR-008 | Assign request to staff member (working title) | P2 | feature | 2 | high |
| FR-009 | Enforce state-transition rules in UpdateState (working title) | P2 | feature | 2 | high |
| FR-010 | Public and internal comments on a request (working title) | P2 | feature | 2 | medium |
| FR-011 | Reject, cancel and close request flows with required reason (working title) | P2 | feature | 3 | medium |
| FR-012 | Concrete state-change observers (notification) registered in DI (working title) | P2 | feature | 2 | medium |
| FR-013 | Staff scope restriction on request access (working title) | P2 | feature | 3 | medium |
| NFR-017 | Authentication (replace hardcoded user123/admin) | P2 | security | 2 | high |
| NFR-018 | Role-based access control (Requester / Staff / Manager) | P2 | security | 2 | high |
| NFR-019 | Object-level access control and secure handling (confirm wording) | P2 | security | 2 | high |
| FR-014 | Management dashboard: request counts by state/category with filters (working title) | P3 | feature | 2 | high |
| FR-015 | Management oversight capability (working title) | P3 | feature | 3 | medium |
| FR-016 | Management slice capability 3 (title from RTM) | P3 | feature | 3 | medium |
| NFR-020 | Audit trail for consequential actions | P3 | feature | 2 | high |
| M3-DATA-01 | Migration: indexes for dashboard and queue queries | P3 | feature | 2 | medium |
| DEF-001 | Services not registered in DI: POST/PUT /api/requests cannot activate the controller | P2 | defect | 2 | high |
| DEF-002 | Initial StatusHistory linked by RequestId before the request has a key (suspected) | P1 | defect | 2 | medium |
| DEF-003 | UpdateState accepts any transition and any caller | P2 | defect | 2 | high |
| DEF-004 | Hardcoded identities (user123 / admin) and no authentication | P2 | defect | 2 | high |
| DEF-005 | Database password committed in appsettings.json | P2 | defect | 1 | medium |
| DEF-006 | Observer failure after SaveChanges returns 500 although the state change is committed | P3 | defect | 3 | medium |
| DEF-007 | Category delete cascades to Requests | P1 | defect | 3 | medium |
| M3-TEST-00 | Repair the test project: references, fixtures, remove placeholder | P2 | test | 1 | high |
| M3-TEST-01 | Test Evidence Record template and storage location | P1 | test | 1 | high |
| M3-TEST-02 | Risk-based test selection | P1 | test | 3 | high |
| M3-TEST-03 | Unit tests: validation and category strategy (TC-U-01, TC-U-02) | P1 | test | 4 | medium |
| M3-TEST-04 | Unit tests: transition rules and role policy (TC-U-03, TC-U-04) | P2 | test | 4 | medium |
| M3-TEST-05 | Unit tests: notifier and dashboard aggregation (TC-U-05, TC-U-06) | P3 | test | 4 | medium |
| M3-TEST-06 | Black-box: equivalence partitioning and BVA on submission fields (TC-B-01..04) | P1 | test | 4 | high |
| M3-TEST-07 | Black-box: state-transition testing (TC-B-05..07) | P1 | test | 4 | high |
| M3-TEST-08 | Black-box: decision table for who can do what (TC-B-08..10) | P1 | test | 4 | medium |
| M3-TEST-09 | API integration: submit request persists request and initial history (TC-I-01) | P2 | test | 4 | high |
| M3-TEST-10 | API integration: invalid category and unauthenticated submit (TC-I-02, TC-I-03) | P2 | test | 4 | high |
| M3-TEST-11 | API integration: state update persists history and notifies; illegal transition rejected (TC-I-04, TC-I-05) | P2 | test | 4 | high |
| M3-TEST-12 | Negative/unauthorised behaviour suite (TC-N-01..03) | P2 | test | 4 | high |
| M3-TEST-13 | E2E journey 1: submit -> staff assigns and resolves -> requester sees update (TC-S-01) | P3 | test | 5 | high |
| M3-TEST-14 | E2E journey 2: resolved request -> feedback -> manager dashboard reflects it (TC-S-02) | P3 | test | 5 | medium |
| M3-TEST-15 | Run critical E2E journey against staging (TC-S-01 on staging) | P3 | test | 5 | high |
| M3-TEST-16 | Regression tests for every fixed defect | P1 | test | 5 | high |
| M3-PERF-01 | Choose performance operation, target and workload | P3 | perf | 3 | medium |
| M3-PERF-02 | Build load script and seed data set | P3 | perf | 5 | medium |
| M3-PERF-03 | Execute, record and interpret the performance result (TC-P-01) | P3 | perf | 5 | high |
| M3-CI-01 | Harden the existing CI workflow | P2 | ci | 3 | high |
| M3-CI-02 | Postgres service container and environment-variable configuration for CI | P2 | ci | 3 | high |
| M3-CI-03 | Quality gate that blocks merging, with evidence of a blocked run | P2 | ci | 3 | high |
| M3-CI-04 | Static analysis and code-style enforcement | P2 | ci | 3 | medium |
| M3-CI-05 | Security, dependency and secrets checks | P2 | security | 3 | medium |
| M3-CI-06 | Coverage collection and an interpretation note | P2 | ci | 4 | medium |
| M3-CI-07 | Static/security findings log and handling | P2 | ci | 5 | medium |
| M3-GOV-01 | Verify and evidence protected main (two non-author approvals) | P2 | process | 1 | high |
| M3-GOV-02 | PR/issue templates and CONTRIBUTING (branch naming, issue links, review checklist) | P2 | process | 1 | medium |
| M3-GOV-03 | Remove committed secrets and tidy configuration | P2 | security | 1 | high |
| M3-GOV-04 | Migration coordination rule to avoid ModelSnapshot conflicts | P2 | process | 1 | high |
| M3-STG-01 | Choose staging approach (credible simulated vs cloud) and record why | P3 | infra | 3 | medium |
| M3-STG-02 | Staging configuration (environment-specific settings, controlled secrets) | P3 | infra | 5 | high |
| M3-STG-03 | Dockerfile and docker-compose for staging (web + PostgreSQL) | P3 | infra | 5 | medium |
| M3-STG-04 | Database migration and seed procedure for staging | P3 | infra | 5 | medium |
| M3-STG-05 | Release identification: version endpoint and build metadata | P3 | infra | 5 | high |
| M3-STG-06 | Staging deploy workflow and smoke test of the deployed version | P3 | infra | 5 | high |
| M3-STG-07 | Health check and basic failure evidence | P3 | infra | 5 | medium |
| M3-STG-08 | Environment differences list (dev vs staging vs intended production) | P3 | docs | 6 | medium |
| M3-STG-09 | Rollback and recovery approach with dry run | P3 | infra | 6 | high |
| M3-RTM-01 | Extend the existing RTM with M3 verification columns | P1 | docs | 1 | high |
| M3-RTM-02 | RTM updates for requester slice (FR-001..006) as tests land | P1 | docs | 4 | high |
| M3-RTM-03 | RTM updates for staff slice and security NFRs (FR-007..013, NFR-017..019) | P2 | docs | 4 | high |
| M3-RTM-04 | RTM updates for management slice, audit and performance (FR-014..016, NFR-020, NFR-024) | P3 | docs | 5 | high |
| M3-DEF-01 | Continue the Defect Register (defect, severity, status, fix, regression evidence) | P1 | docs | 2 | high |
| M3-PED-01 | Create PED v3.0 from v2.0 (version history, change log, evidence-index structure) | P1 | docs | 1 | high |
| M3-PED-02 | PED v3.0 sections 2, 3 (conformance and controlled changes) | P1 | docs | 6 | medium |
| M3-PED-03 | PED v3.0 sections 6, 7, 9 (quality strategy, black-box/integration/system, defects) | P1 | docs | 6 | medium |
| M3-PED-04 | PED v3.0 sections 4, 5 (construction/configuration, CI and automated verification) | P2 | docs | 6 | medium |
| M3-PED-05 | PED v3.0 sections 8, 10 (performance/reliability, release readiness) | P3 | docs | 6 | medium |
| M3-PED-06 | PED v3.0 section 1 overview and submission information block | P1 | docs | 7 | high |
| M3-PED-07 | Harvard references, in-text citations and registers appendix | all | docs | 7 | medium |
| M3-PED-08 | Final proofread, export PED v3.0 to PDF and verify it matches the tagged commit | all | docs | 7 | high |
| M3-DOC-01 | Update README for M3 (status, config, tests, CI, staging, endpoints) | P3 | docs | 6 | medium |
| M3-REL-01 | Release Evidence Summary (build/CI, regression, API, E2E, static/security, performance, defects, acceptance) | P3 | release | 6 | high |
| M3-REL-02 | Readiness classification: READY / CONDITIONALLY READY / NOT READY | P3 | release | 6 | high |
| M3-REL-03 | Residual Risk Statement (real risks only) | P3 | release | 6 | medium |
| M3-REL-04 | Technical Debt Register: create and seed | P3 | docs | 3 | medium |
| M3-REL-05 | Tag the release candidate (only when all three agree PED and RTM match) | all | release | 7 | high |
| M3-REL-06 | Acceptance validation across in-scope requirements | P1 | test | 6 | medium |
| M3-AI-01 | AI Usage Register: log and verify AI use (P1) | P1 | process | 6 | medium |
| M3-RISK-01 | Risk Register update: quality/requirements (P1) | P1 | risk | 6 | medium |
| M3-DEFENCE-01 | Individual defence preparation (P1) | P1 | process | 8 | high |
| M3-AI-02 | AI Usage Register: log and verify AI use (P2) | P2 | process | 6 | medium |
| M3-RISK-02 | Risk Register update: security/process (P2) | P2 | risk | 6 | medium |
| M3-DEFENCE-02 | Individual defence preparation (P2) | P2 | process | 8 | high |
| M3-AI-03 | AI Usage Register: log and verify AI use (P3) | P3 | process | 6 | medium |
| M3-RISK-03 | Risk Register update: release/operations (P3) | P3 | risk | 6 | medium |
| M3-DEFENCE-03 | Individual defence preparation (P3) | P3 | process | 8 | high |
| M3-PRES-01 | Presentation dry run from live artefacts | all | process | 8 | high |

## Labels used

`type:*`, `owner:P1|P2|P3|all`, `area:A..G|X` (M3 rubric areas), `phase:1..8` (work split phases), `priority:high|medium|low`. Milestone: **M3 - Release Candidate**.

## Issue details

### [M3-PLAN-01] Freeze M3 core scope and release-candidate boundary

Labels: `type:process`, `owner:all`, `area:X`, `phase:1`, `priority:high`

**Owner:** all  |  **Phase:** 1  |  **Priority:** high  |  **Rubric area:** Cross-cutting
**Trace:** PED v3.0 s1 | Work plan step 1
**Suggested branch:** `chore/m3-plan-01-freeze-m3-core-scope-and`

### Context
M3 assesses an integrated release candidate covering the *agreed core scope*. Scope must be frozen up front so tests, RTM and PED all describe the same thing.

### Tasks
- [ ] List every FR/NFR ID that is IN the release candidate and every one that is DEFERRED
- [ ] Set RTM status to `In Development` / `Deferred` accordingly (continue the existing RTM, do not recreate it)
- [ ] Copy deferred items into the Technical Debt Register with a reason
- [ ] Agree a scope-freeze date; after it, only defect fixes and tests

### Done when
- [ ] Scope list committed and linked from PED v3.0 s1
- [ ] Every deferred requirement has an RTM status and a debt entry
- [ ] All three members have signed off in the issue
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [M3-PLAN-02] Create GitHub Project board and fill phase dates against Moodle deadline

Labels: `type:process`, `owner:all`, `area:X`, `phase:1`, `priority:medium`

**Owner:** all  |  **Phase:** 1  |  **Priority:** medium  |  **Rubric area:** Cross-cutting
**Trace:** Work split: suggested phase order
**Suggested branch:** `chore/m3-plan-02-create-github-project-board-and`

### Context
The work split leaves phase dates blank. A board with phase columns gives the progressive GitHub evidence the rubric asks for.

### Tasks
- [ ] Add dates for Phases 1-2, 3-4, 5-6, 7-8
- [ ] Create a Project board; add all M3 issues
- [ ] Start CI (Phase 1-3), not last: CI slipping removes everyone's gate

### Done when
- [ ] Dates agreed and visible in README or Project description
- [ ] All M3 issues on the board with owner and phase
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [M3-CONF-01] Build M2-to-implementation conformance table

Labels: `type:docs`, `owner:P1`, `area:A`, `phase:1`, `priority:high`

**Owner:** P1  |  **Phase:** 1  |  **Priority:** high  |  **Rubric area:** A: M2 conformance / implementation
**Trace:** PED v3.0 s2 | Rubric A
**Suggested branch:** `docs/m3-conf-01-build-m2-to-implementation-conformance`

### Context
Show what was implemented as planned and what deviated. Seed rows from the repo as it stands (verify each against PED v2.0): layered Domain/Application/Web/Tests projects exist; PostgreSQL + EF Core with `InitialCreate` migration; Observer = interface + `RequestStateNotifier` only (no concrete observers, not registered in DI); Strategy = `CategoryValidationContext` + interface only (no concrete strategies, not registered in DI); `IRequestRepository`/`ICategoryRepository` are interfaces only and the controller uses `CivicConnectDbContext` directly.

### Tasks
- [ ] Create table: M2 decision | PED/ADR ref | Implemented as planned? | Evidence (file/PR) | Deviation? | ADR/change ref
- [ ] Flag only *significant* deviations (minor coding changes need no ADR)
- [ ] Update the table as the slices land, not at the end

### Done when
- [ ] Table in PED v3.0 s2 with repo evidence links
- [ ] Each significant deviation links to an ADR/change record
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [M3-CONF-02] ADR-M3-01: Repository abstraction vs direct DbContext use

Labels: `type:adr`, `owner:P1`, `area:A`, `phase:1`, `priority:medium`

**Owner:** P1  |  **Phase:** 1  |  **Priority:** medium  |  **Rubric area:** A: M2 conformance / implementation
**Trace:** PED v3.0 s3 | Rubric A | M2 persistence/design baseline
**Depends on:** M3-CONF-01
**Suggested branch:** `docs/m3-conf-02-adr-m3-01-repository-abstraction`

### Context
M2 defines repository interfaces but `RequestsController` talks to `CivicConnectDbContext` directly. Decide: implement repositories (and an application service layer) or revise the decision and document EF Core as the unit of work.

### Tasks
- [ ] Record: original decision -> implementation evidence/problem -> revised decision -> consequence
- [ ] Consider testability (unit tests need mockable seams), complexity, and schedule
- [ ] Only raise if the deviation is confirmed with the team

### Done when
- [ ] ADR committed with status and consequences
- [ ] Conformance table row links to it
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [M3-CONF-03] ADR-M3-02: Authentication and authorisation approach

Labels: `type:adr`, `owner:P1`, `area:A`, `phase:1`, `priority:high`

**Owner:** P1  |  **Phase:** 1  |  **Priority:** high  |  **Rubric area:** A: M2 conformance / implementation
**Trace:** NFR-017/018/019 | PED v3.0 s3
**Suggested branch:** `docs/m3-conf-03-adr-m3-02-authentication-and`

### Context
`Program.cs` calls `UseAuthorization()` but there is no authentication scheme, and the controller hardcodes `user123` / `admin`. Decide the approach (e.g. ASP.NET Core Identity with cookies, JWT bearer, or a simulated-role scheme for staging) before the staff slice builds on it. P2 implements; P1 records the decision with P2's input.

### Tasks
- [ ] List alternatives with cost/complexity/security trade-offs
- [ ] Decide how test users and roles are provisioned in test and staging
- [ ] Record consequences for tests (how tests authenticate)

### Done when
- [ ] ADR committed before FR-007+ work starts
- [ ] NFR-017/018/019 issues reference it
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [M3-CONF-04] ADR-M3-03: Request state-transition model and permitted-transition table

Labels: `type:adr`, `owner:P1`, `area:A`, `phase:1`, `priority:high`

**Owner:** P1  |  **Phase:** 1  |  **Priority:** high  |  **Rubric area:** A: M2 conformance / implementation
**Trace:** Request lifecycle | PED v3.0 s3
**Suggested branch:** `docs/m3-conf-04-adr-m3-03-request-state`

### Context
`RequestState` has 7 states (Open, Assigned, InProgress, Resolved, Closed, Cancelled, Rejected) but `UpdateState` accepts any new state. Define which transitions are legal, who may trigger each, and where the rule lives (domain method vs service). This table is also the test basis for state-transition testing.

### Tasks
- [ ] Draw the state diagram and permitted-transition table (from, to, role, reason required?)
- [ ] Decide where enforcement lives
- [ ] Decide whether Resolved->InProgress (reopen) is allowed, per the AC

### Done when
- [ ] Table committed in PED/ADR
- [ ] FR-009 and TEST-07 reference it
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [FR-001] Submit service request (validated, authenticated, DTO response)

Labels: `type:feature`, `owner:P1`, `area:A`, `phase:2`, `priority:high`

**Owner:** P1  |  **Phase:** 2  |  **Priority:** high  |  **Rubric area:** A: M2 conformance / implementation
**Trace:** FR-001 | Requester slice | ASR: data integrity
**Depends on:** M3-CONF-03, DEF-001, DEF-002
**Suggested branch:** `feature/fr-001-submit-service-request`

### Context
`POST /api/requests` exists but: RequesterId is hardcoded to `user123`; DTO has no validation attributes; the response returns the EF entity; and the initial `StatusHistory` is linked via `RequestId = request.Id` before the key exists (see DEF-002).

### Tasks
- [ ] Add validation rules per M1 acceptance criteria (title/description limits, required category) and return 400 ProblemDetails
- [ ] Take requester identity from the authenticated principal (depends on NFR-017)
- [ ] Return a response DTO, not the entity
- [ ] Add the initial history row via the `request.History` navigation
- [ ] Keep category validation through `CategoryValidationContext`

### Done when
- [ ] Valid submission returns 201 + Location + reference
- [ ] Invalid input returns 400 with field errors and persists nothing
- [ ] One Request (State=Open) and one StatusHistory row are persisted
- [ ] Tests: TC-B-01..04, TC-I-01
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [FR-002] Category selection and category-specific validation strategies

Labels: `type:feature`, `owner:P1`, `area:A`, `phase:2`, `priority:high`

**Owner:** P1  |  **Phase:** 2  |  **Priority:** high  |  **Rubric area:** A: M2 conformance / implementation
**Trace:** FR-002 | M2 design decision: Strategy
**Depends on:** DEF-001
**Suggested branch:** `feature/fr-002-category-selection-and-category-specific`

### Context
Categories are seeded (Infrastructure, Utilities, Parks) and `ICategoryValidationStrategy` exists, but no concrete strategy or category-listing endpoint exists. Strategy needs visible application evidence.

### Tasks
- [ ] Implement `GET /api/categories`
- [ ] Implement concrete strategies for categories that genuinely need different rules (per AC), otherwise rely on the documented default
- [ ] Register strategies in DI (see DEF-001)

### Done when
- [ ] Categories endpoint returns the seeded set
- [ ] At least one concrete strategy is used by the running system
- [ ] Unit tests TC-U-01/02 pass
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [FR-003] Unique, human-readable request reference ID

Labels: `type:feature`, `owner:P1`, `area:A`, `phase:2`, `priority:medium`

**Owner:** P1  |  **Phase:** 2  |  **Priority:** medium  |  **Rubric area:** A: M2 conformance / implementation
**Trace:** FR-003
**Depends on:** M3-GOV-04, M3-DATA-01
**Suggested branch:** `feature/fr-003-unique-human-readable-request-reference`

### Context
Requests only have an integer identity `Id`. **Confirm FR-003's acceptance criterion first**: if the numeric id satisfies it, close this issue with RTM evidence; if a reference like `CC-2026-000123` is required, implement it.

### Tasks
- [ ] Add `ReferenceNumber` with a unique index via a migration (follow the migration rule, M3-GOV-04)
- [ ] Generate deterministically from the identity/sequence, not random
- [ ] Return it on submission and in lookups

### Done when
- [ ] References unique at DB level
- [ ] Visible in the submission response and the status view
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [FR-004] Requester views own request and status

Labels: `type:feature`, `owner:P1`, `area:A`, `phase:2`, `priority:high`

**Owner:** P1  |  **Phase:** 2  |  **Priority:** high  |  **Rubric area:** A: M2 conformance / implementation
**Trace:** FR-004 | NFR-019
**Depends on:** NFR-017, NFR-019
**Suggested branch:** `feature/fr-004-requester-views-own-request-and`

### Context
`GET /api/requests/{id}` has no ownership check and returns the raw entity.

### Tasks
- [ ] Enforce that a requester can only read their own requests
- [ ] Add `GET /api/requests/mine` (paged)
- [ ] Return a DTO with the state as a readable name
- [ ] Never include internal comments

### Done when
- [ ] Owner sees own request; another requester gets 403/404
- [ ] Tests: TC-B-08..10, TC-N-02
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [FR-005] Requester views status history timeline

Labels: `type:feature`, `owner:P1`, `area:A`, `phase:2`, `priority:medium`

**Owner:** P1  |  **Phase:** 2  |  **Priority:** medium  |  **Rubric area:** A: M2 conformance / implementation
**Trace:** FR-005
**Depends on:** FR-004
**Suggested branch:** `feature/fr-005-requester-views-status-history-timeline`

### Context
`StatusHistory` rows are written but nothing exposes them.

### Tasks
- [ ] Add `GET /api/requests/{id}/history` ordered by `ChangedAt`
- [ ] Include the initial submission entry
- [ ] Confirm with the AC whether `Reason` is shown to requesters

### Done when
- [ ] History matches the transitions performed
- [ ] Ownership enforced as in FR-004
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [FR-006] Requester feedback on a completed request

Labels: `type:feature`, `owner:P1`, `area:A`, `phase:3`, `priority:medium`

**Owner:** P1  |  **Phase:** 3  |  **Priority:** medium  |  **Rubric area:** A: M2 conformance / implementation
**Trace:** FR-006
**Depends on:** M3-GOV-04, FR-004
**Suggested branch:** `feature/fr-006-requester-feedback-on-a-completed`

### Context
No feedback entity exists in the model or migration. **Confirm FR-006 fields from the RTM** before building.

### Tasks
- [ ] Add a `Feedback` entity (RequestId, rating/comment per AC, CreatedAt, CreatedBy) and a migration
- [ ] `POST /api/requests/{id}/feedback`: owner only, only when Resolved/Closed, one per request
- [ ] Update `db_schema.sql` per the migration rule

### Done when
- [ ] Feedback rejected on open requests and for non-owners
- [ ] Second submission rejected
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [M3-UI-01] Decide and build minimal UI (or documented API-only demo path) for E2E journeys

Labels: `type:feature`, `owner:P3`, `area:A`, `phase:2`, `priority:high`

**Owner:** P3  |  **Phase:** 2  |  **Priority:** high  |  **Rubric area:** A: M2 conformance / implementation
**Trace:** M2 UI baseline | E2E journeys
**Suggested branch:** `feature/m3-ui-01-decide-and-build-minimal-ui`

### Context
`Program.cs` maps `{controller=Home}/{action=Index}` and uses MVC with views, but the repo has no `HomeController`, no views and an empty `wwwroot`, so `/` returns 404. The E2E tests need a driver. Team decision first: minimal Razor/static pages, or API-only E2E (HttpClient + Swagger demo).

### Tasks
- [ ] Decide and record (change record if it deviates from M2)
- [ ] If UI: submit form, my requests list, staff queue, status update
- [ ] If API-only: remove the unused default route and document the demo path

### Done when
- [ ] `/` no longer 404s or the API-only decision is documented
- [ ] E2E journeys TC-S-01/02 can run end to end
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [FR-007] Staff request queue with filters (working title)

Labels: `type:feature`, `owner:P2`, `area:A`, `phase:2`, `priority:high`

**Owner:** P2  |  **Phase:** 2  |  **Priority:** high  |  **Rubric area:** A: M2 conformance / implementation
**Trace:** FR-007..013 (Staff slice) | Working title: map to the exact FR-007..013 IDs in the RTM before creating this issue.
**Depends on:** NFR-018
**Suggested branch:** `feature/fr-007-staff-request-queue-with-filters`

### Context
There is no staff-facing listing yet.

### Tasks
- [ ] `GET /api/staff/requests?state=&categoryId=&assigneeId=` with paging
- [ ] Staff/Manager roles only
- [ ] Project to a list DTO

### Done when
- [ ] Filters combine correctly
- [ ] Requesters get 403
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [FR-008] Assign request to staff member (working title)

Labels: `type:feature`, `owner:P2`, `area:A`, `phase:2`, `priority:high`

**Owner:** P2  |  **Phase:** 2  |  **Priority:** high  |  **Rubric area:** A: M2 conformance / implementation
**Trace:** FR-007..013 | Working title: map to the exact FR-007..013 IDs in the RTM before creating this issue.
**Depends on:** FR-009, NFR-020
**Suggested branch:** `feature/fr-008-assign-request-to-staff-member`

### Context
`Request.AssigneeId` exists; no endpoint sets it.

### Tasks
- [ ] `PUT /api/requests/{id}/assign`
- [ ] Open->Assigned through the transition rules
- [ ] Write StatusHistory and an audit entry
- [ ] Notify observers

### Done when
- [ ] Assignee and state updated atomically
- [ ] History row records actor and time
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [FR-009] Enforce state-transition rules in UpdateState (working title)

Labels: `type:feature`, `owner:P2`, `area:A`, `phase:2`, `priority:high`

**Owner:** P2  |  **Phase:** 2  |  **Priority:** high  |  **Rubric area:** A: M2 conformance / implementation
**Trace:** FR-007..013 | ADR-M3-03 | Working title: map to the exact FR-007..013 IDs in the RTM before creating this issue.
**Depends on:** M3-CONF-04, NFR-017, DEF-003
**Suggested branch:** `feature/fr-009-enforce-state-transition-rules-in`

### Context
`UpdateState` currently sets any state, records `ChangedBy = "admin"` and never checks legality (DEF-003).

### Tasks
- [ ] Implement the permitted-transition table from ADR-M3-03
- [ ] Require a reason where specified (Rejected/Cancelled)
- [ ] Return 409/400 on illegal transitions
- [ ] Take the actor from the authenticated user
- [ ] Call observers after a successful save

### Done when
- [ ] Illegal transitions rejected and state unchanged
- [ ] Every legal transition writes exactly one history row
- [ ] Tests: TC-U-03, TC-B-05..07, TC-I-04/05
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [FR-010] Public and internal comments on a request (working title)

Labels: `type:feature`, `owner:P2`, `area:A`, `phase:2`, `priority:medium`

**Owner:** P2  |  **Phase:** 2  |  **Priority:** medium  |  **Rubric area:** A: M2 conformance / implementation
**Trace:** FR-007..013 | Working title: map to the exact FR-007..013 IDs in the RTM before creating this issue.
**Depends on:** NFR-017, NFR-018
**Suggested branch:** `feature/fr-010-public-and-internal-comments-on`

### Context
`Comment` has `IsInternal` and a table, but no endpoints exist.

### Tasks
- [ ] `POST /api/requests/{id}/comments` for staff (internal or public); requesters may add public comments if the AC allows
- [ ] Requester views must filter out `IsInternal` comments
- [ ] Take `CreatedBy` from the principal

### Done when
- [ ] Internal comment never visible to the requester
- [ ] Covered by a negative test
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [FR-011] Reject, cancel and close request flows with required reason (working title)

Labels: `type:feature`, `owner:P2`, `area:A`, `phase:3`, `priority:medium`

**Owner:** P2  |  **Phase:** 3  |  **Priority:** medium  |  **Rubric area:** A: M2 conformance / implementation
**Trace:** FR-007..013 | Working title: map to the exact FR-007..013 IDs in the RTM before creating this issue.
**Depends on:** FR-009
**Suggested branch:** `feature/fr-011-reject-cancel-and-close-request`

### Context
Rejected, Cancelled and Closed exist in the enum but have no behaviour.

### Accepted implementation
- `Open` and `Assigned` may transition to `Rejected` or `Cancelled`; `InProgress` may also transition to `Closed`; `Resolved` may transition to `Closed`.
- `Rejected`, `Cancelled`, and `Closed` are terminal states. Every transition into one requires a non-blank reason persisted in `StatusHistory`.
- Staff, managers, and admins may reject or close. Requesters may cancel their own requests; staff, managers, and admins may cancel any request they can access.
- Enforcement lives in `RequestStateTransitionPolicy`, called by `RequestsController.UpdateState`.

### Tasks
- [x] Define who may reject/cancel/close and from which states (ADR-M3-03)
- [x] Require and persist a reason
- [x] Terminal states accept no further transitions

### Done when
- [x] Terminal-state rules enforced and tested
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [FR-012] Concrete state-change observers (notification) registered in DI (working title)

Labels: `type:feature`, `owner:P2`, `area:A`, `phase:2`, `priority:medium`

**Owner:** P2  |  **Phase:** 2  |  **Priority:** medium  |  **Rubric area:** A: M2 conformance / implementation
**Trace:** FR-007..013 | M2 design decision: Observer | Working title: map to the exact FR-007..013 IDs in the RTM before creating this issue.
**Depends on:** DEF-001, DEF-006
**Suggested branch:** `feature/fr-012-concrete-state-change-observers-registered`

### Context
`RequestStateNotifier` and `IRequestStateObserver` exist but no observer implements the interface, so the pattern has no running evidence.

### Tasks
- [ ] Implement at least one real observer (e.g. a notification log/outbox record)
- [ ] Register all observers in DI
- [ ] Isolate observer failures (DEF-006)

### Done when
- [ ] A state change causes a visible observer effect
- [ ] Unit test TC-U-05 passes
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [FR-013] Staff scope restriction on request access (working title)

Labels: `type:feature`, `owner:P2`, `area:A`, `phase:3`, `priority:medium`

**Owner:** P2  |  **Phase:** 3  |  **Priority:** medium  |  **Rubric area:** A: M2 conformance / implementation
**Trace:** FR-007..013 | NFR-019 | Working title: map to the exact FR-007..013 IDs in the RTM before creating this issue.
**Depends on:** NFR-018
**Suggested branch:** `feature/fr-013-staff-scope-restriction-on-request`

### Context
Needed for the 'staff accessing out-of-scope request' negative case. Confirm the scope rule (assigned-only, category, or all staff) from the AC.

### Accepted implementation
- Staff access is assigned-only: staff may access requests where `AssigneeId` matches their authenticated user ID.
- Admins and managers retain broad access. Requesters may access their own requests.
- `AccessControlService` is the single authorization check; controller denial returns `403 Forbidden`.

### Tasks
- [x] Implement the confirmed scope rule in a single authorisation policy/handler
- [x] Return 403 for out-of-scope access

### Done when
- [x] TC-N-03 passes against the real endpoint
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [NFR-017] Authentication (replace hardcoded user123/admin)

Labels: `type:security`, `owner:P2`, `area:A`, `phase:2`, `priority:high`

**Owner:** P2  |  **Phase:** 2  |  **Priority:** high  |  **Rubric area:** A: M2 conformance / implementation
**Trace:** NFR-017 | ADR-M3-02 | DEF-004
**Depends on:** M3-CONF-03
**Suggested branch:** `feature/nfr-017-authentication`

### Context
No `UseAuthentication()` and no scheme are configured; identities are string literals in the controller.

### Tasks
- [ ] Implement the approach chosen in ADR-M3-02
- [ ] Seed test users via environment/config, never committed credentials
- [ ] Replace `user123`/`admin` with the principal's identity
- [ ] Unauthenticated calls return 401

### Done when
- [ ] No hardcoded identities remain
- [ ] TC-I-03 passes
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [NFR-018] Role-based access control (Requester / Staff / Manager)

Labels: `type:security`, `owner:P2`, `area:A`, `phase:2`, `priority:high`

**Owner:** P2  |  **Phase:** 2  |  **Priority:** high  |  **Rubric area:** A: M2 conformance / implementation
**Trace:** NFR-018
**Depends on:** NFR-017
**Suggested branch:** `feature/nfr-018-role-based-access-control`

### Context
No roles or policies exist.

### Tasks
- [ ] Define roles and central authorisation policies
- [ ] Apply policies to every endpoint; default-deny
- [ ] Document the role x action matrix (input to the decision-table tests)

### Done when
- [ ] Role matrix committed
- [ ] TC-N-01 passes
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [NFR-019] Object-level access control and secure handling (confirm wording)

Labels: `type:security`, `owner:P2`, `area:A`, `phase:2`, `priority:high`

**Owner:** P2  |  **Phase:** 2  |  **Priority:** high  |  **Rubric area:** A: M2 conformance / implementation
**Trace:** NFR-019 | **Confirm NFR-019's exact wording in the RTM**
**Depends on:** NFR-018
**Suggested branch:** `feature/nfr-019-object-level-access-control-and`

### Context
Ensures a user can only touch the objects they are entitled to; also covers any secure-handling criteria in the AC.

### Tasks
- [ ] Centralise ownership checks in a reusable handler
- [ ] Verify the AC for NFR-019 and implement anything beyond ownership

### Done when
- [ ] Cross-user access attempts fail; tests TC-N-02/03
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [FR-014] Management dashboard: request counts by state/category with filters (working title)

Labels: `type:feature`, `owner:P3`, `area:A`, `phase:2`, `priority:high`

**Owner:** P3  |  **Phase:** 2  |  **Priority:** high  |  **Rubric area:** A: M2 conformance / implementation
**Trace:** FR-014 | NFR-024 | Working title: confirm against RTM.
**Depends on:** NFR-018, M3-DATA-01
**Suggested branch:** `feature/fr-014-management-dashboard-request-counts-by`

### Context
Dashboard filtering is the performance-sensitive operation named against NFR-024's 3-second target.

### Tasks
- [ ] Aggregation endpoint (counts by state, category, age bucket) with filters
- [ ] Manager role only
- [ ] Query shaped to use the indexes from M3-DATA-01

### Done when
- [ ] Counts reconcile with the underlying data
- [ ] Response time target measured in M3-PERF-03
- [ ] TC-U-06
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [FR-015] Management oversight capability (working title)

Labels: `type:feature`, `owner:P3`, `area:A`, `phase:3`, `priority:medium`

**Owner:** P3  |  **Phase:** 3  |  **Priority:** medium  |  **Rubric area:** A: M2 conformance / implementation
**Trace:** FR-015 | Working title: confirm against RTM.
**Depends on:** NFR-020
**Suggested branch:** `feature/fr-015-management-oversight-capability`

### Context
Set title and criteria from the RTM (e.g. reassign/escalate stalled requests or reporting).

### Tasks
- [ ] Implement per the FR-015 acceptance criteria
- [ ] Writes audit entries (NFR-020)

### Done when
- [ ] AC demonstrated and traced in the RTM
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [FR-016] Management slice capability 3 (title from RTM)

Labels: `type:feature`, `owner:P3`, `area:A`, `phase:3`, `priority:medium`

**Owner:** P3  |  **Phase:** 3  |  **Priority:** medium  |  **Rubric area:** A: M2 conformance / implementation
**Trace:** FR-016 | Working title: confirm against RTM.
**Suggested branch:** `feature/fr-016-management-slice-capability-3`

### Context
Placeholder: replace the title and tasks with FR-016's real wording before creating this issue.

### Tasks
- [ ] Implement per FR-016 acceptance criteria

### Done when
- [ ] AC demonstrated and traced in the RTM
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [NFR-020] Audit trail for consequential actions

Labels: `type:feature`, `owner:P3`, `area:A`, `phase:2`, `priority:high`

**Owner:** P3  |  **Phase:** 2  |  **Priority:** high  |  **Rubric area:** A: M2 conformance / implementation
**Trace:** NFR-020
**Depends on:** M3-GOV-04, NFR-018
**Suggested branch:** `feature/nfr-020-audit-trail-for-consequential-actions`

### Context
`StatusHistory` is requester-facing history; the audit trail is a separate, append-only record of who did what.

### Tasks
- [ ] Add `AuditLog` (actor, action, entity type/id, before/after summary, timestamp, correlation id) and a migration
- [ ] Write entries for state changes, assignment and role-protected actions
- [ ] Manager-only read endpoint; no update/delete path

### Done when
- [ ] Each consequential action produces exactly one audit entry
- [ ] Non-managers cannot read the log
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [M3-DATA-01] Migration: indexes for dashboard and queue queries

Labels: `type:feature`, `owner:P3`, `area:F`, `phase:2`, `priority:medium`

**Owner:** P3  |  **Phase:** 2  |  **Priority:** medium  |  **Rubric area:** F: Performance and staging
**Trace:** NFR-024 | PED v3.0 s8
**Depends on:** M3-GOV-04
**Suggested branch:** `feature/m3-data-01-migration-indexes-for-dashboard-and`

### Context
The model only indexes foreign keys. Dashboard/queue filters hit State, RequesterId, AssigneeId and CreatedAt.

### Tasks
- [ ] Add indexes via one migration PR (see M3-GOV-04)
- [ ] Regenerate `db_schema.sql`
- [ ] Record before/after query evidence for the performance test

### Done when
- [ ] Migration applies cleanly on an empty and a seeded DB
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [DEF-001] Services not registered in DI: POST/PUT /api/requests cannot activate the controller

Labels: `type:defect`, `owner:P2`, `area:E`, `phase:2`, `priority:high`

**Owner:** P2  |  **Phase:** 2  |  **Priority:** high  |  **Rubric area:** E: RTM, defects, traceability
**Trace:** Severity: High | FR-001, FR-009, FR-012 | Found in code review of the uploaded repo snapshot; reproduce first, then log in the Defect Register.
**Suggested branch:** `fix/def-001-services-not-registered-in-di`

### Context
`RequestsController` requires `CategoryValidationContext` and `RequestStateNotifier`, which need `IEnumerable<ICategoryValidationStrategy>` and `IEnumerable<IRequestStateObserver>`. `Program.cs` registers none of these.

### Tasks
- [ ] Reproduce (expect an InvalidOperationException on controller activation)
- [ ] Register context, notifier, strategies and observers
- [ ] Add regression test (integration test hitting the endpoint)

### Done when
- [ ] Endpoints respond; regression test merged
- [ ] Defect Register row closed with fix + test reference
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [DEF-002] Initial StatusHistory linked by RequestId before the request has a key (suspected)

Labels: `type:defect`, `owner:P1`, `area:E`, `phase:2`, `priority:medium`

**Owner:** P1  |  **Phase:** 2  |  **Priority:** medium  |  **Rubric area:** E: RTM, defects, traceability
**Trace:** Severity: Medium | FR-001, FR-005 | Found in code review of the uploaded repo snapshot; reproduce first, then log in the Defect Register.
**Suggested branch:** `fix/def-002-initial-statushistory-linked-by-requestid`

### Context
`CreateRequest` sets `RequestId = request.Id` while `Id` is still 0/temporary and does not add the row through the `Request.History` navigation. This may cause an FK failure or an orphaned history row.

### Tasks
- [ ] Write the failing test first to confirm the behaviour
- [ ] Fix by adding the row via `request.History.Add(...)`
- [ ] Keep the test as regression

### Done when
- [ ] Persisted history row references the created request
- [ ] Defect Register updated (or marked Not a defect with evidence)
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [DEF-003] UpdateState accepts any transition and any caller

Labels: `type:defect`, `owner:P2`, `area:E`, `phase:2`, `priority:high`

**Owner:** P2  |  **Phase:** 2  |  **Priority:** high  |  **Rubric area:** E: RTM, defects, traceability
**Trace:** Severity: High | FR-009 | Found in code review of the uploaded repo snapshot; reproduce first, then log in the Defect Register.
**Suggested branch:** `fix/def-003-updatestate-accepts-any-transition-and`

### Context
`PUT /api/requests/{id}/state` can move Closed->Open, skips reason checks and has no authorisation.

### Tasks
- [ ] Reproduce with a request in a terminal state
- [ ] Fixed by FR-009 and NFR-018
- [ ] Regression tests TC-I-05, TC-B-06

### Done when
- [ ] Illegal transition rejected, state unchanged
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [DEF-004] Hardcoded identities (user123 / admin) and no authentication

Labels: `type:defect`, `owner:P2`, `area:E`, `phase:2`, `priority:high`

**Owner:** P2  |  **Phase:** 2  |  **Priority:** high  |  **Rubric area:** E: RTM, defects, traceability
**Trace:** Severity: Critical | NFR-017, NFR-018 | Found in code review of the uploaded repo snapshot; reproduce first, then log in the Defect Register.
**Suggested branch:** `fix/def-004-hardcoded-identities-and-no-authentication`

### Context
Any caller acts as `admin` and every submission belongs to `user123`; ownership rules cannot work.

### Tasks
- [ ] Fixed by NFR-017/018
- [ ] Regression test: unauthenticated and wrong-role calls fail

### Done when
- [ ] No literal identities remain in controllers
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [DEF-005] Database password committed in appsettings.json

Labels: `type:defect`, `owner:P2`, `area:B`, `phase:1`, `priority:medium`

**Owner:** P2  |  **Phase:** 1  |  **Priority:** medium  |  **Rubric area:** B: Construction and configuration
**Trace:** Severity: Medium | Secrets rule | Found in code review of the uploaded repo snapshot; reproduce first, then log in the Defect Register.
**Suggested branch:** `fix/def-005-database-password-committed-in-appsettings`

### Context
`ConnectionStrings:DefaultConnection` contains `Username=postgres;Password=postgres`. It is a dev default, but the brief says secrets stay out of the repo and this will be the first thing a reviewer sees.

### Tasks
- [ ] Resolved by M3-GOV-03
- [ ] Record in the Defect Register with the outcome

### Done when
- [ ] No credentials in tracked config
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [DEF-006] Observer failure after SaveChanges returns 500 although the state change is committed

Labels: `type:defect`, `owner:P3`, `area:E`, `phase:3`, `priority:medium`

**Owner:** P3  |  **Phase:** 3  |  **Priority:** medium  |  **Rubric area:** E: RTM, defects, traceability
**Trace:** Severity: Medium | FR-012 | Found in code review of the uploaded repo snapshot; reproduce first, then log in the Defect Register.
**Suggested branch:** `fix/def-006-observer-failure-after-savechanges-returns`

### Context
`NotifyStateChangedAsync` runs after commit and is unguarded; one failing observer turns a successful change into an error response.

### Tasks
- [ ] Test with a throwing observer (Moq)
- [ ] Catch/log per observer so one failure cannot affect the rest or the response
- [ ] Record a Technical Debt entry if a proper outbox is deferred

### Done when
- [ ] Failing observer does not change the HTTP result; regression test merged
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [DEF-007] Category delete cascades to Requests

Labels: `type:defect`, `owner:P1`, `area:E`, `phase:3`, `priority:medium`

**Owner:** P1  |  **Phase:** 3  |  **Priority:** medium  |  **Rubric area:** E: RTM, defects, traceability
**Trace:** Severity: Medium | Data integrity | Found in code review of the uploaded repo snapshot; reproduce first, then log in the Defect Register.
**Depends on:** M3-GOV-04
**Suggested branch:** `fix/def-007-category-delete-cascades-to-requests`

### Context
The `Requests -> Categories` FK uses `ON DELETE CASCADE`: removing a category would delete every request in it.

### Tasks
- [ ] Change to `Restrict` via a new migration (follow M3-GOV-04)
- [ ] Add a test or document that categories are never deleted

### Done when
- [ ] Migration merged; schema script regenerated
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [M3-TEST-00] Repair the test project: references, fixtures, remove placeholder

Labels: `type:test`, `owner:P2`, `area:C`, `phase:1`, `priority:high`

**Owner:** P2  |  **Phase:** 1  |  **Priority:** high  |  **Rubric area:** C: Automation and CI
**Trace:** Rubric C | Meaningful Test Rule
**Suggested branch:** `test/m3-test-00-repair-the-test-project-references`

### Context
`CivicConnect.Tests.csproj` has no `ProjectReference` to Domain/Application/Web, and `UnitTest1.Test1` is empty (it would count as padding).

### Tasks
- [ ] Add project references
- [ ] Add `Microsoft.AspNetCore.Mvc.Testing`; expose `public partial class Program`
- [ ] Create a `WebApplicationFactory` with test DB configuration and test-authentication helper
- [ ] Delete `UnitTest1.cs`
- [ ] Add shared test-data builders

### Done when
- [ ] `dotnet test` runs real tests in CI
- [ ] No empty or trivial tests remain
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [M3-TEST-01] Test Evidence Record template and storage location

Labels: `type:test`, `owner:P1`, `area:D`, `phase:1`, `priority:high`

**Owner:** P1  |  **Phase:** 1  |  **Priority:** high  |  **Rubric area:** D: Black-box / integration / system
**Trace:** Brief s11
**Suggested branch:** `test/m3-test-01-test-evidence-record-template-and`

### Context
Every counted test needs: basis, why selected, technique/level, input, expected (derived before execution), actual, status, evidence, trace, interpretation.

### Tasks
- [ ] Create the template (markdown/CSV) in the repo
- [ ] Define Test IDs: TC-U-nn, TC-B-nn, TC-I-nn, TC-S-nn, TC-N-nn, TC-P-nn
- [ ] Decide how each record links to the CI run and the RTM row

### Done when
- [ ] Template merged; first records filled before the tests are executed
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [M3-TEST-02] Risk-based test selection

Labels: `type:test`, `owner:P1`, `area:C`, `phase:3`, `priority:high`

**Owner:** P1  |  **Phase:** 3  |  **Priority:** high  |  **Rubric area:** C: Automation and CI
**Trace:** PED v3.0 s6 | Week 5 SO25
**Suggested branch:** `test/m3-test-02-risk-based-test-selection`

### Context
Choose which behaviours get deeper verification and say why. Candidates from the repo: state-transition legality, authorisation, requester data isolation, validation, audit/observer side effects.

### Tasks
- [ ] Rank risks by likelihood x impact using the Risk Register
- [ ] Map the top risks to test cases
- [ ] Make sure >=5 cases address high-priority requirements/risks

### Done when
- [ ] Selection table in PED s6 with links to TC IDs
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [M3-TEST-03] Unit tests: validation and category strategy (TC-U-01, TC-U-02)

Labels: `type:test`, `owner:P1`, `area:C`, `phase:4`, `priority:medium`

**Owner:** P1  |  **Phase:** 4  |  **Priority:** medium  |  **Rubric area:** C: Automation and CI
**Trace:** FR-001, FR-002
**Depends on:** M3-TEST-00, FR-002
**Suggested branch:** `test/m3-test-03-unit-tests-validation-and-category`

### Context
Component-level checks of business rules in the requester slice.

### Tasks
- [ ] TC-U-01: default validation rejects blank title/description and accepts valid input
- [ ] TC-U-02: strategy selected by CategoryId decides the result (Moq strategy returning false -> invalid)

### Done when
- [ ] Real assertions; expected results written before execution; Test Evidence Records filled
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [M3-TEST-04] Unit tests: transition rules and role policy (TC-U-03, TC-U-04)

Labels: `type:test`, `owner:P2`, `area:C`, `phase:4`, `priority:medium`

**Owner:** P2  |  **Phase:** 4  |  **Priority:** medium  |  **Rubric area:** C: Automation and CI
**Trace:** FR-009, NFR-018
**Depends on:** M3-TEST-00, FR-009, NFR-018
**Suggested branch:** `test/m3-test-04-unit-tests-transition-rules-and`

### Context
Unit-level protection of the highest-risk rules.

### Tasks
- [ ] TC-U-03: Open->Assigned allowed; Closed->Open rejected
- [ ] TC-U-04: Requester role fails the staff policy

### Done when
- [ ] Real assertions; Test Evidence Records filled
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [M3-TEST-05] Unit tests: notifier and dashboard aggregation (TC-U-05, TC-U-06)

Labels: `type:test`, `owner:P3`, `area:C`, `phase:4`, `priority:medium`

**Owner:** P3  |  **Phase:** 4  |  **Priority:** medium  |  **Rubric area:** C: Automation and CI
**Trace:** FR-012, FR-014
**Depends on:** M3-TEST-00, FR-012, FR-014
**Suggested branch:** `test/m3-test-05-unit-tests-notifier-and-dashboard`

### Context
Covers the Observer behaviour and dashboard calculations.

### Tasks
- [ ] TC-U-05: notifier calls every registered observer once with the old and new state
- [ ] TC-U-06: aggregation groups counts correctly for a controlled data set

### Done when
- [ ] Real assertions; Test Evidence Records filled
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [M3-TEST-06] Black-box: equivalence partitioning and BVA on submission fields (TC-B-01..04)

Labels: `type:test`, `owner:P1`, `area:D`, `phase:4`, `priority:high`

**Owner:** P1  |  **Phase:** 4  |  **Priority:** high  |  **Rubric area:** D: Black-box / integration / system
**Trace:** FR-001 | Technique: EP + BVA
**Depends on:** FR-001
**Suggested branch:** `test/m3-test-06-black-box-equivalence-partitioning-and`

### Context
Text-length limits and required fields are classic EP/BVA material. Take the actual limits from the M1 acceptance criteria; do not invent numbers.

### Tasks
- [ ] Partition title/description/category inputs
- [ ] Boundaries: below-min, min, max, above-max (per AC)
- [ ] Derive expected results from the AC before executing

### Done when
- [ ] At least 4 cases traced to AC; technique justification written in the Test Evidence Records
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [M3-TEST-07] Black-box: state-transition testing (TC-B-05..07)

Labels: `type:test`, `owner:P1`, `area:D`, `phase:4`, `priority:high`

**Owner:** P1  |  **Phase:** 4  |  **Priority:** high  |  **Rubric area:** D: Black-box / integration / system
**Trace:** FR-009 | ADR-M3-03 | Technique: state transition
**Depends on:** M3-CONF-04, FR-009
**Suggested branch:** `test/m3-test-07-black-box-state-transition-testing`

### Context
Behaviour depends on current state, so state-transition testing is justified.

### Tasks
- [ ] TC-B-05: valid path Open->Assigned->InProgress->Resolved->Closed
- [ ] TC-B-06: invalid transition from a terminal state rejected
- [ ] TC-B-07: reason-required transitions (Rejected/Cancelled) without a reason rejected

### Done when
- [ ] Cases derive from the ADR-M3-03 table; results recorded
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [M3-TEST-08] Black-box: decision table for who can do what (TC-B-08..10)

Labels: `type:test`, `owner:P1`, `area:D`, `phase:4`, `priority:medium`

**Owner:** P1  |  **Phase:** 4  |  **Priority:** medium  |  **Rubric area:** D: Black-box / integration / system
**Trace:** NFR-018, NFR-019 | Technique: decision table
**Depends on:** NFR-018, NFR-019
**Suggested branch:** `test/m3-test-08-black-box-decision-table-for`

### Context
Role x ownership x state interact to decide access: a decision table fits.

### Tasks
- [ ] Build the table (role, owns request?, state -> allowed/denied)
- [ ] Select rows that cover each distinct outcome (avoid duplicates)

### Done when
- [ ] Table in PED s7 and cases traced to rows
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [M3-TEST-09] API integration: submit request persists request and initial history (TC-I-01)

Labels: `type:test`, `owner:P2`, `area:D`, `phase:4`, `priority:high`

**Owner:** P2  |  **Phase:** 4  |  **Priority:** high  |  **Rubric area:** D: Black-box / integration / system
**Trace:** FR-001, DEF-001, DEF-002
**Depends on:** M3-TEST-00, M3-CI-02, FR-001
**Suggested branch:** `test/m3-test-09-api-integration-submit-request-persists`

### Context
Runs the real pipeline against a relational database (InMemory does not enforce FKs, so note its limits if used).

### Tasks
- [ ] Use `WebApplicationFactory` with a Postgres test database (CI service container)
- [ ] Assert 201, response DTO, DB rows (Request + 1 StatusHistory)

### Done when
- [ ] Passes in CI
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [M3-TEST-10] API integration: invalid category and unauthenticated submit (TC-I-02, TC-I-03)

Labels: `type:test`, `owner:P2`, `area:D`, `phase:4`, `priority:high`

**Owner:** P2  |  **Phase:** 4  |  **Priority:** high  |  **Rubric area:** D: Black-box / integration / system
**Trace:** FR-001, NFR-017
**Depends on:** M3-TEST-09, NFR-017
**Suggested branch:** `test/m3-test-10-api-integration-invalid-category-and`

### Context
Negative/failure cases at the API boundary.

### Tasks
- [ ] TC-I-02: unknown CategoryId -> 400 and nothing persisted
- [ ] TC-I-03: no credentials -> 401

### Done when
- [ ] Both pass in CI; counted as negative cases
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [M3-TEST-11] API integration: state update persists history and notifies; illegal transition rejected (TC-I-04, TC-I-05)

Labels: `type:test`, `owner:P2`, `area:D`, `phase:4`, `priority:high`

**Owner:** P2  |  **Phase:** 4  |  **Priority:** high  |  **Rubric area:** D: Black-box / integration / system
**Trace:** FR-009, FR-012
**Depends on:** M3-TEST-09, FR-009, FR-012
**Suggested branch:** `test/m3-test-11-api-integration-state-update-persists`

### Context
Verifies the boundary between controller, persistence and observer.

### Tasks
- [ ] TC-I-04: valid transition -> 200, history row, observer invoked
- [ ] TC-I-05: illegal transition -> 4xx, state and history unchanged

### Done when
- [ ] Both pass in CI
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [M3-TEST-12] Negative/unauthorised behaviour suite (TC-N-01..03)

Labels: `type:test`, `owner:P2`, `area:D`, `phase:4`, `priority:high`

**Owner:** P2  |  **Phase:** 4  |  **Priority:** high  |  **Rubric area:** D: Black-box / integration / system
**Trace:** NFR-018, NFR-019, FR-013
**Depends on:** NFR-018, NFR-019, FR-013
**Suggested branch:** `test/m3-test-12-negative-unauthorised-behaviour-suite`

### Context
Requirement: >=3 negative cases across the suite. P2 writes with P1.

### Tasks
- [ ] TC-N-01: requester calls a staff endpoint -> 403
- [ ] TC-N-02: requester reads another requester's request -> 403/404
- [ ] TC-N-03: staff accesses out-of-scope request -> 403

### Done when
- [ ] Cases pass; overlap with other TCs is stated explicitly
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [M3-TEST-13] E2E journey 1: submit -> staff assigns and resolves -> requester sees update (TC-S-01)

Labels: `type:test`, `owner:P3`, `area:D`, `phase:5`, `priority:high`

**Owner:** P3  |  **Phase:** 5  |  **Priority:** high  |  **Rubric area:** D: Black-box / integration / system
**Trace:** FR-001, FR-004, FR-005, FR-008, FR-009
**Depends on:** M3-UI-01, FR-001, FR-009
**Suggested branch:** `test/m3-test-13-e2e-journey-1-submit-staff`

### Context
Critical journey across requester and staff slices.

### Tasks
- [ ] Script the journey (UI or API-driven per M3-UI-01)
- [ ] Assert the requester's view and history after each step

### Done when
- [ ] Passes locally and is wired into CI where feasible
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [M3-TEST-14] E2E journey 2: resolved request -> feedback -> manager dashboard reflects it (TC-S-02)

Labels: `type:test`, `owner:P3`, `area:D`, `phase:5`, `priority:medium`

**Owner:** P3  |  **Phase:** 5  |  **Priority:** medium  |  **Rubric area:** D: Black-box / integration / system
**Trace:** FR-006, FR-014
**Depends on:** FR-006, FR-014
**Suggested branch:** `test/m3-test-14-e2e-journey-2-resolved-request`

### Context
Second critical journey; adjust to whichever journeys the team freezes in M3-PLAN-01.

### Tasks
- [ ] Script and execute
- [ ] Assert dashboard counts changed as expected

### Done when
- [ ] Passes locally
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [M3-TEST-15] Run critical E2E journey against staging (TC-S-01 on staging)

Labels: `type:test`, `owner:P3`, `area:F`, `phase:5`, `priority:high`

**Owner:** P3  |  **Phase:** 5  |  **Priority:** high  |  **Rubric area:** F: Performance and staging
**Trace:** Brief s14
**Depends on:** M3-TEST-13, M3-STG-06
**Suggested branch:** `test/m3-test-15-run-critical-e2e-journey-against`

### Context
Demonstrates the deployed release candidate works, not just local code.

### Tasks
- [ ] Point the E2E suite at the staging base URL
- [ ] Capture run output tied to the deployed version

### Done when
- [ ] Result recorded with the version identifier
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [M3-TEST-16] Regression tests for every fixed defect

Labels: `type:test`, `owner:P1`, `area:C`, `phase:5`, `priority:high`

**Owner:** P1  |  **Phase:** 5  |  **Priority:** high  |  **Rubric area:** C: Automation and CI
**Trace:** Brief s9 regression rule
**Suggested branch:** `test/m3-test-16-regression-tests-for-every-fixed`

### Context
Each closed DEF-nnn must keep a test that would fail if the defect returned.

### Tasks
- [ ] Map DEF-001..007 to regression tests
- [ ] Link them in the Defect Register and RTM

### Done when
- [ ] Every closed defect has a test reference
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [M3-PERF-01] Choose performance operation, target and workload

Labels: `type:performance`, `owner:P3`, `area:F`, `phase:3`, `priority:medium`

**Owner:** P3  |  **Phase:** 3  |  **Priority:** medium  |  **Rubric area:** F: Performance and staging
**Trace:** NFR-024 | Brief s13
**Suggested branch:** `test/m3-perf-01-choose-performance-operation-target-and`

### Context
Pick ONE operation (dashboard filtering or request submission), justify it, and define a small workload. If NFR-024's 3-second target applies to the chosen operation, use it; otherwise state that no formal target exists.

### Tasks
- [ ] Write the rationale
- [ ] Choose the tool (e.g. k6, JMeter, NBomber) and workload (users, duration, data volume)
- [ ] Define pass/fail target

### Done when
- [ ] Plan committed before executing
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [M3-PERF-02] Build load script and seed data set

Labels: `type:performance`, `owner:P3`, `area:F`, `phase:5`, `priority:medium`

**Owner:** P3  |  **Phase:** 5  |  **Priority:** medium  |  **Rubric area:** F: Performance and staging
**Trace:** Brief s13
**Depends on:** M3-PERF-01, M3-STG-03
**Suggested branch:** `test/m3-perf-02-build-load-script-and-seed`

### Context
Deterministic data and a repeatable script.

### Tasks
- [ ] Seed script for the chosen data volume
- [ ] Load script with authentication
- [ ] Run against the staging-like environment

### Done when
- [ ] Script and seed in the repo; run instructions documented
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [M3-PERF-03] Execute, record and interpret the performance result (TC-P-01)

Labels: `type:performance`, `owner:P3`, `area:F`, `phase:5`, `priority:high`

**Owner:** P3  |  **Phase:** 5  |  **Priority:** high  |  **Rubric area:** F: Performance and staging
**Trace:** PED v3.0 s8
**Depends on:** M3-PERF-02
**Suggested branch:** `test/m3-perf-03-execute-record-and-interpret-the`

### Context
Evidence must include environment, data, workload, duration, response times and error rate.

### Tasks
- [ ] Run and capture p50/p95 and error rate
- [ ] Compare with the target
- [ ] Write what this does NOT prove about municipal production scale

### Done when
- [ ] PED s8 complete with limitations stated
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [M3-CI-01] Harden the existing CI workflow

Labels: `type:ci`, `owner:P2`, `area:C`, `phase:3`, `priority:high`

**Owner:** P2  |  **Phase:** 3  |  **Priority:** high  |  **Rubric area:** C: Automation and CI
**Trace:** PED v3.0 s5 | Rubric C
**Depends on:** M3-TEST-00
**Suggested branch:** `ci/m3-ci-01-harden-the-existing-ci-workflow`

### Context
`.github/workflows/test.yml` builds three projects one by one and runs tests. Its content is indented by two spaces throughout (valid but untidy), there is no caching, no test result artefact, no least-privilege permissions and no analysis or security step.

### Tasks
- [ ] Build the solution once (`src/CivicConnect.slnx`) then `dotnet test --no-build`
- [ ] Add NuGet caching and `permissions: contents: read`
- [ ] Publish TRX results as an artefact (and coverage, see M3-CI-06)
- [ ] Add concurrency to cancel superseded runs

### Done when
- [ ] Green run on PR and push with downloadable results
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [M3-CI-02] Postgres service container and environment-variable configuration for CI

Labels: `type:ci`, `owner:P2`, `area:C`, `phase:3`, `priority:high`

**Owner:** P2  |  **Phase:** 3  |  **Priority:** high  |  **Rubric area:** C: Automation and CI
**Trace:** Brief s9 API/integration
**Depends on:** M3-CI-01, M3-GOV-03
**Suggested branch:** `ci/m3-ci-02-postgres-service-container-and-environment`

### Context
Integration tests need a relational DB, and secrets must come from env vars, not appsettings.

### Tasks
- [ ] Add a `postgres` service with health check
- [ ] Pass `ConnectionStrings__DefaultConnection` via env
- [ ] Apply migrations before tests

### Done when
- [ ] Integration tests pass in CI without any committed credentials
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [M3-CI-03] Quality gate that blocks merging, with evidence of a blocked run

Labels: `type:ci`, `owner:P2`, `area:C`, `phase:3`, `priority:high`

**Owner:** P2  |  **Phase:** 3  |  **Priority:** high  |  **Rubric area:** C: Automation and CI
**Trace:** Brief s9 quality gate
**Depends on:** M3-CI-01, M3-GOV-01
**Suggested branch:** `ci/m3-ci-03-quality-gate-that-blocks-merging`

### Context
At least one meaningful failure must block progression.

### Tasks
- [ ] Mark CI as a required status check in the `main` ruleset
- [ ] Document gate rules: build fail, test fail, analyser errors, high vulnerabilities
- [ ] Keep a real or deliberate failing PR as evidence (a red run that blocked merge)
- [ ] Policy: no rerun-until-green for flaky tests

### Done when
- [ ] Link to a blocked PR in PED s5
- [ ] Gate rules written in PED s5
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [M3-CI-04] Static analysis and code-style enforcement

Labels: `type:ci`, `owner:P2`, `area:C`, `phase:3`, `priority:medium`

**Owner:** P2  |  **Phase:** 3  |  **Priority:** medium  |  **Rubric area:** C: Automation and CI
**Trace:** Brief s9 static analysis
**Depends on:** M3-CI-01
**Suggested branch:** `ci/m3-ci-04-static-analysis-and-code-style`

### Context
Enable the .NET analysers and format check (and optionally CodeQL for C#).

### Tasks
- [ ] Add `Directory.Build.props` with analysis level and `EnforceCodeStyleInBuild`
- [ ] Add `dotnet format --verify-no-changes` step
- [ ] Triage the first findings: fix, suppress with reason, or log

### Done when
- [ ] Step runs in CI; first-run findings handled and recorded in M3-CI-07
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [M3-CI-05] Security, dependency and secrets checks

Labels: `type:security`, `owner:P2`, `area:C`, `phase:3`, `priority:medium`

**Owner:** P2  |  **Phase:** 3  |  **Priority:** medium  |  **Rubric area:** C: Automation and CI
**Trace:** Brief s9 security checks
**Depends on:** M3-CI-01
**Suggested branch:** `feature/m3-ci-05-security-dependency-and-secrets-checks`

### Context
Use GitHub-supported options.

### Tasks
- [ ] Enable Dependabot (NuGet + GitHub Actions)
- [ ] Add `dotnet list package --vulnerable --include-transitive` step that fails on high
- [ ] Enable secret scanning and push protection
- [ ] Run a history scan for the committed password (DEF-005)

### Done when
- [ ] Checks active and visible in the repo settings/CI
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [M3-CI-06] Coverage collection and an interpretation note

Labels: `type:ci`, `owner:P2`, `area:C`, `phase:4`, `priority:medium`

**Owner:** P2  |  **Phase:** 4  |  **Priority:** medium  |  **Rubric area:** C: Automation and CI
**Trace:** Brief s9 coverage rule
**Depends on:** M3-CI-01
**Suggested branch:** `ci/m3-ci-06-coverage-collection-and-an-interpretation`

### Context
`coverlet.collector` is already referenced. Report coverage but set no arbitrary threshold; explain what it does and does not show.

### Tasks
- [ ] Collect Cobertura output in CI and upload it
- [ ] Write the note: executed lines != verified behaviour; uncovered critical paths listed

### Done when
- [ ] Coverage artefact available; interpretation note in PED s5
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [M3-CI-07] Static/security findings log and handling

Labels: `type:ci`, `owner:P2`, `area:E`, `phase:5`, `priority:medium`

**Owner:** P2  |  **Phase:** 5  |  **Priority:** medium  |  **Rubric area:** E: RTM, defects, traceability
**Trace:** Brief s12
**Depends on:** M3-CI-04, M3-CI-05
**Suggested branch:** `ci/m3-ci-07-static-security-findings-log-and`

### Context
Assessors want important findings and how they were handled.

### Tasks
- [ ] Log each finding: tool, severity, decision (fixed/mitigated/accepted), owner
- [ ] Link accepted items to the Technical Debt or Residual Risk entries

### Done when
- [ ] Log committed and referenced from PED s5/s9
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [M3-GOV-01] Verify and evidence protected main (two non-author approvals)

Labels: `type:process`, `owner:P2`, `area:B`, `phase:1`, `priority:high`

**Owner:** P2  |  **Phase:** 1  |  **Priority:** high  |  **Rubric area:** B: Construction and configuration
**Trace:** Rubric B | README Governance section
**Suggested branch:** `chore/m3-gov-01-verify-and-evidence-protected-main`

### Context
The README states `main` is protected; capture it as evidence rather than assuming.

### Tasks
- [ ] Ruleset: PR required, 2 approvals, dismiss stale approvals, require CI, block force-push, no bypass
- [ ] Export/screenshot the ruleset as supporting evidence
- [ ] Check that no substantive commit landed directly on `main`

### Done when
- [ ] Settings evidence linked from PED s4
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [M3-GOV-02] PR/issue templates and CONTRIBUTING (branch naming, issue links, review checklist)

Labels: `type:process`, `owner:P2`, `area:B`, `phase:1`, `priority:medium`

**Owner:** P2  |  **Phase:** 1  |  **Priority:** medium  |  **Rubric area:** B: Construction and configuration
**Trace:** Rubric B
**Suggested branch:** `chore/m3-gov-02-pr-issue-templates-and-contributing`

### Context
Reviews must show actual checking. Branch convention (from the work split): `feature/...`, `docs/...`, plus `test/` and `ci/` as needed.

### Tasks
- [ ] PR template: linked issue, AC covered, tests added, RTM updated, AI used + verified
- [ ] Review checklist: what the reviewer actually ran/checked
- [ ] CONTRIBUTING.md: branch naming, commit messages referencing issue IDs

### Done when
- [ ] Templates active; first PR uses them
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [M3-GOV-03] Remove committed secrets and tidy configuration

Labels: `type:security`, `owner:P2`, `area:B`, `phase:1`, `priority:high`

**Owner:** P2  |  **Phase:** 1  |  **Priority:** high  |  **Rubric area:** B: Construction and configuration
**Trace:** DEF-005 | Rubric B
**Suggested branch:** `feature/m3-gov-03-remove-committed-secrets-and-tidy`

### Context
Plaintext DB credentials are in `appsettings.json`, and `CivicConnect.Web.csproj.user` (a personal VS file) is tracked.

### Tasks
- [ ] Replace with a placeholder/empty value; read from `ConnectionStrings__DefaultConnection`
- [ ] Add `.env.example`; document user-secrets for local dev
- [ ] Add `*.user` to `.gitignore` and untrack the file
- [ ] Update the README setup steps

### Done when
- [ ] No credentials in tracked files; README matches
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [M3-GOV-04] Migration coordination rule to avoid ModelSnapshot conflicts

Labels: `type:process`, `owner:P2`, `area:B`, `phase:1`, `priority:high`

**Owner:** P2  |  **Phase:** 1  |  **Priority:** high  |  **Rubric area:** B: Construction and configuration
**Trace:** Rubric B | Risk Register (process)
**Suggested branch:** `chore/m3-gov-04-migration-coordination-rule-to-avoid`

### Context
Several issues add migrations in parallel (FR-003, FR-006, NFR-020, DATA-01, DEF-007). `CivicConnectDbContextModelSnapshot.cs` conflicts badly when two branches add migrations.

### Tasks
- [ ] Rule: one migration per PR; rebase on main before `dotnet ef migrations add`
- [ ] Agree an order for the pending migrations
- [ ] Regenerate `db_schema.sql` (`dotnet ef migrations script --idempotent`) in each migration PR

### Done when
- [ ] Rule in CONTRIBUTING; order recorded in this issue
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [M3-STG-01] Choose staging approach (credible simulated vs cloud) and record why

Labels: `type:infra`, `owner:P3`, `area:F`, `phase:3`, `priority:medium`

**Owner:** P3  |  **Phase:** 3  |  **Priority:** medium  |  **Rubric area:** F: Performance and staging
**Trace:** Brief s14 | PED v3.0 s8
**Suggested branch:** `feature/m3-stg-01-choose-staging-approach-and-record`

### Context
No bonus marks for infrastructure complexity; pick what is demonstrable and honest.

### Tasks
- [ ] Compare options (compose-based local staging vs hosted)
- [ ] Record the decision and consequences in PED

### Done when
- [ ] Decision recorded; demo path identified for the assessor
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [M3-STG-02] Staging configuration (environment-specific settings, controlled secrets)

Labels: `type:infra`, `owner:P3`, `area:F`, `phase:5`, `priority:high`

**Owner:** P3  |  **Phase:** 5  |  **Priority:** high  |  **Rubric area:** F: Performance and staging
**Trace:** Brief s14
**Depends on:** M3-GOV-03
**Suggested branch:** `feature/m3-stg-02-staging-configuration`

### Context
`Program.cs` only exposes Swagger in Development. Decide if staging needs Swagger for the demo (feature flag) and keep all secrets in env vars.

### Tasks
- [ ] Add `appsettings.Staging.json` (non-secret values only)
- [ ] Run with `ASPNETCORE_ENVIRONMENT=Staging`
- [ ] Document each variable in the README

### Done when
- [ ] Staging runs with no secrets in the repo
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [M3-STG-03] Dockerfile and docker-compose for staging (web + PostgreSQL)

Labels: `type:infra`, `owner:P3`, `area:F`, `phase:5`, `priority:medium`

**Owner:** P3  |  **Phase:** 5  |  **Priority:** medium  |  **Rubric area:** F: Performance and staging
**Trace:** Brief s14
**Depends on:** M3-STG-02
**Suggested branch:** `feature/m3-stg-03-dockerfile-and-docker-compose-for`

### Context
Gives a repeatable, separate-from-dev environment.

### Tasks
- [ ] Dockerfile for `CivicConnect.Web`
- [ ] `docker-compose.staging.yml` with Postgres, health checks and a volume
- [ ] `.dockerignore`

### Done when
- [ ] `docker compose up` yields a working staging stack
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [M3-STG-04] Database migration and seed procedure for staging

Labels: `type:infra`, `owner:P3`, `area:F`, `phase:5`, `priority:medium`

**Owner:** P3  |  **Phase:** 5  |  **Priority:** medium  |  **Rubric area:** F: Performance and staging
**Trace:** Brief s14
**Depends on:** M3-STG-03
**Suggested branch:** `feature/m3-stg-04-database-migration-and-seed-procedure`

### Context
The app does not apply migrations on startup, so the procedure must be explicit.

### Tasks
- [ ] Choose EF migration bundle or the idempotent SQL script
- [ ] Seed staging users/data via env-driven script
- [ ] Document the order of operations

### Done when
- [ ] Fresh staging build reaches a usable state with a documented procedure
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [M3-STG-05] Release identification: version endpoint and build metadata

Labels: `type:infra`, `owner:P3`, `area:F`, `phase:5`, `priority:high`

**Owner:** P3  |  **Phase:** 5  |  **Priority:** high  |  **Rubric area:** F: Performance and staging
**Trace:** Brief s14 'correct release candidate is deployed'
**Depends on:** M3-CI-01
**Suggested branch:** `feature/m3-stg-05-release-identification-version-endpoint-and`

### Context
The assessor must be able to confirm the deployed build equals the tagged commit.

### Tasks
- [ ] Inject git SHA/tag at CI build time
- [ ] Expose it at `GET /api/version` (or the health endpoint)
- [ ] Tag the build artefact/image with the same SHA

### Done when
- [ ] Running app reports the exact RC identifier
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [M3-STG-06] Staging deploy workflow and smoke test of the deployed version

Labels: `type:infra`, `owner:P3`, `area:F`, `phase:5`, `priority:high`

**Owner:** P3  |  **Phase:** 5  |  **Priority:** high  |  **Rubric area:** F: Performance and staging
**Trace:** Brief s14
**Depends on:** M3-STG-03, M3-STG-05
**Suggested branch:** `feature/m3-stg-06-staging-deploy-workflow-and-smoke`

### Context
Manual `workflow_dispatch` or script that deploys a chosen tag, then proves it.

### Tasks
- [ ] Deploy the tag to staging
- [ ] Smoke test: version equals the tag, a health check is green, one read endpoint works

### Done when
- [ ] Run evidence linked from PED s8/s10
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [M3-STG-07] Health check and basic failure evidence

Labels: `type:infra`, `owner:P3`, `area:F`, `phase:5`, `priority:medium`

**Owner:** P3  |  **Phase:** 5  |  **Priority:** medium  |  **Rubric area:** F: Performance and staging
**Trace:** PED v3.0 s8
**Depends on:** M3-STG-03
**Suggested branch:** `feature/m3-stg-07-health-check-and-basic-failure`

### Context
The brief asks for relevant failure evidence in the reliability section.

### Tasks
- [ ] Add `/health` with a DB check and baseline logging
- [ ] Demonstrate DB-down behaviour (unhealthy, sensible error response)

### Done when
- [ ] Failure evidence captured with environment and date
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [M3-STG-08] Environment differences list (dev vs staging vs intended production)

Labels: `type:docs`, `owner:P3`, `area:F`, `phase:6`, `priority:medium`

**Owner:** P3  |  **Phase:** 6  |  **Priority:** medium  |  **Rubric area:** F: Performance and staging
**Trace:** Brief s14
**Suggested branch:** `docs/m3-stg-08-environment-differences-list`

### Context
Name the differences that could change production behaviour (data volume, Swagger, secrets, hosting, TLS, auth users).

### Tasks
- [ ] Create the table
- [ ] Link each material difference to a residual risk where relevant

### Done when
- [ ] Table in PED s8/s10
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [M3-STG-09] Rollback and recovery approach with dry run

Labels: `type:infra`, `owner:P3`, `area:F`, `phase:6`, `priority:high`

**Owner:** P3  |  **Phase:** 6  |  **Priority:** high  |  **Rubric area:** F: Performance and staging
**Trace:** Brief s14
**Depends on:** M3-STG-06
**Suggested branch:** `feature/m3-stg-09-rollback-and-recovery-approach-with`

### Context
Say what artefact is restored and the data/config concerns. Note that EF `Down()` for `InitialCreate` drops tables, so the data risk must be addressed (backup/restore or forward-fix).

### Tasks
- [ ] Write the procedure (previous tag/image, DB backup, config)
- [ ] Do one dry run on staging and record the result

### Done when
- [ ] Procedure and dry-run evidence in PED s10
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [M3-RTM-01] Extend the existing RTM with M3 verification columns

Labels: `type:docs`, `owner:P1`, `area:E`, `phase:1`, `priority:high`

**Owner:** P1  |  **Phase:** 1  |  **Priority:** high  |  **Rubric area:** E: RTM, defects, traceability
**Trace:** PED v3.0 | Rubric E
**Suggested branch:** `docs/m3-rtm-01-extend-the-existing-rtm-with`

### Context
Continue the same RTM; do not recreate it. Add: Test case ID(s), Expected result, Execution evidence (CI run link), Defect ref, Status.

### Tasks
- [ ] Add the columns in a PR (history must show evolution)
- [ ] Define status values (Planned / In Development / Implemented / Verified / Changed / Deferred)

### Done when
- [ ] Columns present and populated for the first requirement
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [M3-RTM-02] RTM updates for requester slice (FR-001..006) as tests land

Labels: `type:docs`, `owner:P1`, `area:E`, `phase:4`, `priority:high`

**Owner:** P1  |  **Phase:** 4  |  **Priority:** high  |  **Rubric area:** E: RTM, defects, traceability
**Trace:** Rubric E
**Depends on:** M3-RTM-01
**Suggested branch:** `docs/m3-rtm-02-rtm-updates-for-requester-slice`

### Context
Update in the same PRs that add the tests, not at the end.

### Tasks
- [ ] Fill AC -> implementation -> test -> expected -> evidence -> defect/status per row

### Done when
- [ ] Each row traceable end to end; PR history shows incremental updates
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [M3-RTM-03] RTM updates for staff slice and security NFRs (FR-007..013, NFR-017..019)

Labels: `type:docs`, `owner:P2`, `area:E`, `phase:4`, `priority:high`

**Owner:** P2  |  **Phase:** 4  |  **Priority:** high  |  **Rubric area:** E: RTM, defects, traceability
**Trace:** Rubric E
**Depends on:** M3-RTM-01
**Suggested branch:** `docs/m3-rtm-03-rtm-updates-for-staff-slice`

### Context
Same rule as M3-RTM-02.

### Tasks
- [ ] Fill the rows per requirement as PRs merge

### Done when
- [ ] Rows complete and consistent with test evidence
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [M3-RTM-04] RTM updates for management slice, audit and performance (FR-014..016, NFR-020, NFR-024)

Labels: `type:docs`, `owner:P3`, `area:E`, `phase:5`, `priority:high`

**Owner:** P3  |  **Phase:** 5  |  **Priority:** high  |  **Rubric area:** E: RTM, defects, traceability
**Trace:** Rubric E
**Depends on:** M3-RTM-01
**Suggested branch:** `docs/m3-rtm-04-rtm-updates-for-management-slice`

### Context
Same rule as M3-RTM-02.

### Tasks
- [ ] Fill rows including the performance evidence link for NFR-024

### Done when
- [ ] Rows complete and consistent with test evidence
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [M3-DEF-01] Continue the Defect Register (defect, severity, status, fix, regression evidence)

Labels: `type:docs`, `owner:P1`, `area:E`, `phase:2`, `priority:high`

**Owner:** P1  |  **Phase:** 2  |  **Priority:** high  |  **Rubric area:** E: RTM, defects, traceability
**Trace:** Brief s12
**Suggested branch:** `docs/m3-def-01-continue-the-defect-register`

### Context
Create it in its controlled location and seed it with DEF-001..007 from this list.

### Tasks
- [ ] Create the register file and fields
- [ ] Keep status in sync with GitHub issues
- [ ] Summarise open defects for the release review

### Done when
- [ ] Register linked from PED s9
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [M3-PED-01] Create PED v3.0 from v2.0 (version history, change log, evidence-index structure)

Labels: `type:docs`, `owner:P1`, `area:X`, `phase:1`, `priority:high`

**Owner:** P1  |  **Phase:** 1  |  **Priority:** high  |  **Rubric area:** Cross-cutting
**Trace:** Brief s6 | Do NOT create a new PED
**Suggested branch:** `docs/m3-ped-01-create-ped-v3-0-from`

### Context
Continue the same document; preserve history.

### Tasks
- [ ] Update document control and version history
- [ ] Add the s1-s10 structure and appendices/links
- [ ] Keep it an evidence index, linking to repo/CI instead of duplicating

### Done when
- [ ] PED v3.0 draft in repo history, continuing v2.0
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [M3-PED-02] PED v3.0 sections 2, 3 (conformance and controlled changes)

Labels: `type:docs`, `owner:P1`, `area:A`, `phase:6`, `priority:medium`

**Owner:** P1  |  **Phase:** 6  |  **Priority:** medium  |  **Rubric area:** A: M2 conformance / implementation
**Trace:** PED v3.0 s2-3
**Depends on:** M3-CONF-01
**Suggested branch:** `docs/m3-ped-02-ped-v3-0-sections-2`

### Context
Fill from M3-CONF-01 and the ADRs.

### Tasks
- [ ] Write the sections with evidence links

### Done when
- [ ] Reviewed by both other members
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [M3-PED-03] PED v3.0 sections 6, 7, 9 (quality strategy, black-box/integration/system, defects)

Labels: `type:docs`, `owner:P1`, `area:D`, `phase:6`, `priority:medium`

**Owner:** P1  |  **Phase:** 6  |  **Priority:** medium  |  **Rubric area:** D: Black-box / integration / system
**Trace:** PED v3.0 s6, s7, s9
**Depends on:** M3-TEST-02
**Suggested branch:** `docs/m3-ped-03-ped-v3-0-sections-6`

### Context
Interpret results, not just list them.

### Tasks
- [ ] Write the sections with links to TC records, CI runs and the Defect Register

### Done when
- [ ] Reviewed by both other members
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [M3-PED-04] PED v3.0 sections 4, 5 (construction/configuration, CI and automated verification)

Labels: `type:docs`, `owner:P2`, `area:B`, `phase:6`, `priority:medium`

**Owner:** P2  |  **Phase:** 6  |  **Priority:** medium  |  **Rubric area:** B: Construction and configuration
**Trace:** PED v3.0 s4-5
**Depends on:** M3-CI-03
**Suggested branch:** `docs/m3-ped-04-ped-v3-0-sections-4`

### Context
Include gate rules and the blocked-run evidence.

### Tasks
- [ ] Write the sections with links

### Done when
- [ ] Reviewed by both other members
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [M3-PED-05] PED v3.0 sections 8, 10 (performance/reliability, release readiness)

Labels: `type:docs`, `owner:P3`, `area:F`, `phase:6`, `priority:medium`

**Owner:** P3  |  **Phase:** 6  |  **Priority:** medium  |  **Rubric area:** F: Performance and staging
**Trace:** PED v3.0 s8, s10
**Depends on:** M3-PERF-03, M3-REL-01
**Suggested branch:** `docs/m3-ped-05-ped-v3-0-sections-8`

### Context
Depends on a stable release candidate.

### Tasks
- [ ] Write the sections with links

### Done when
- [ ] Reviewed by both other members
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [M3-PED-06] PED v3.0 section 1 overview and submission information block

Labels: `type:docs`, `owner:P1`, `area:X`, `phase:7`, `priority:high`

**Owner:** P1  |  **Phase:** 7  |  **Priority:** high  |  **Rubric area:** Cross-cutting
**Trace:** Brief s5.3
**Depends on:** M3-REL-05
**Suggested branch:** `docs/m3-ped-06-ped-v3-0-section-1`

### Context
Team, repo URL, RC tag/commit, PED version, RTM link, CI/test evidence link, staging URL or demo instructions, date.

### Tasks
- [ ] Fill the block after the RC is tagged

### Done when
- [ ] All items present and correct
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [M3-PED-07] Harvard references, in-text citations and registers appendix

Labels: `type:docs`, `owner:all`, `area:X`, `phase:7`, `priority:medium`

**Owner:** all  |  **Phase:** 7  |  **Priority:** medium  |  **Rubric area:** Cross-cutting
**Trace:** Brief s24 | Academic integrity
**Suggested branch:** `docs/m3-ped-07-harvard-references-in-text-citations`

### Context
Missing in-text citations/references scores zero.

### Tasks
- [ ] Add references for every external claim
- [ ] Link RTM, Risk, ADR, Debt, Defect and AI registers in the appendix

### Done when
- [ ] Citations verified
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [M3-PED-08] Final proofread, export PED v3.0 to PDF and verify it matches the tagged commit

Labels: `type:docs`, `owner:all`, `area:X`, `phase:7`, `priority:high`

**Owner:** all  |  **Phase:** 7  |  **Priority:** high  |  **Rubric area:** Cross-cutting
**Trace:** Brief s5.4
**Depends on:** M3-REL-05
**Suggested branch:** `docs/m3-ped-08-final-proofread-export-ped-v3`

### Context
The assessed version is the identified tag/commit.

### Tasks
- [ ] Joint proofread
- [ ] Export PDF
- [ ] Check that every link and claim corresponds to the RC

### Done when
- [ ] PDF ready
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [M3-DOC-01] Update README for M3 (status, config, tests, CI, staging, endpoints)

Labels: `type:docs`, `owner:P3`, `area:B`, `phase:6`, `priority:medium`

**Owner:** P3  |  **Phase:** 6  |  **Priority:** medium  |  **Rubric area:** B: Construction and configuration
**Trace:** M2 s8 application documentation
**Suggested branch:** `docs/m3-doc-01-update-readme-for-m3`

### Context
README still describes only M2 status and `dotnet ef database update` with a literal password.

### Tasks
- [ ] Update implementation status and run/test instructions
- [ ] Document env vars, staging, endpoints, module responsibilities and known limitations
- [ ] Link to PED/RTM

### Done when
- [ ] A new developer can run the project from the README alone
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [M3-REL-01] Release Evidence Summary (build/CI, regression, API, E2E, static/security, performance, defects, acceptance)

Labels: `type:release`, `owner:P3`, `area:G`, `phase:6`, `priority:high`

**Owner:** P3  |  **Phase:** 6  |  **Priority:** high  |  **Rubric area:** G: Release review and residual risk
**Trace:** Brief s15
**Depends on:** M3-PERF-03, M3-TEST-15
**Suggested branch:** `release/m3-rel-01-release-evidence-summary`

### Context
One concise table: result, interpretation, concern/action per evidence area.

### Tasks
- [ ] Create it and update it as evidence arrives
- [ ] Link each row to CI runs or reports

### Done when
- [ ] Complete and consistent with the RTM
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [M3-REL-02] Readiness classification: READY / CONDITIONALLY READY / NOT READY

Labels: `type:release`, `owner:P3`, `area:G`, `phase:6`, `priority:high`

**Owner:** P3  |  **Phase:** 6  |  **Priority:** high  |  **Rubric area:** G: Release review and residual risk
**Trace:** Brief s15
**Depends on:** M3-REL-01
**Suggested branch:** `release/m3-rel-02-readiness-classification-ready-conditionally-ready`

### Context
The classification earns little; the reasoning earns the marks.

### Tasks
- [ ] State blockers and accepted limitations
- [ ] Justify the classification with several evidence sources

### Done when
- [ ] All three members can defend it
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [M3-REL-03] Residual Risk Statement (real risks only)

Labels: `type:release`, `owner:P3`, `area:G`, `phase:6`, `priority:medium`

**Owner:** P3  |  **Phase:** 6  |  **Priority:** medium  |  **Rubric area:** G: Release review and residual risk
**Trace:** Brief s16
**Depends on:** M3-STG-08
**Suggested branch:** `release/m3-rel-03-residual-risk-statement`

### Context
Why it remains, current mitigation, next action and owner. Do not invent risks.

### Tasks
- [ ] Draw on environment differences, untested paths, accepted findings and deferred debt

### Done when
- [ ] Each risk has an owner and an action
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [M3-REL-04] Technical Debt Register: create and seed

Labels: `type:docs`, `owner:P3`, `area:E`, `phase:3`, `priority:medium`

**Owner:** P3  |  **Phase:** 3  |  **Priority:** medium  |  **Rubric area:** E: RTM, defects, traceability
**Trace:** Brief s12
**Suggested branch:** `docs/m3-rel-04-technical-debt-register-create-and`

### Context
Seed from what the code shows (verify): repository abstraction bypassed (if not resolved), no optimistic concurrency token on Request, observer notification not outbox-based, no pagination on some reads, Swagger only in Development, deferred FRs.

### Tasks
- [ ] Create the register with item, impact, interest, plan, owner
- [ ] Keep it updated as debt is accepted

### Done when
- [ ] Register linked from PED appendices
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [M3-REL-05] Tag the release candidate (only when all three agree PED and RTM match)

Labels: `type:release`, `owner:all`, `area:G`, `phase:7`, `priority:high`

**Owner:** all  |  **Phase:** 7  |  **Priority:** high  |  **Rubric area:** G: Release review and residual risk
**Trace:** Brief s5.1 | s5.4
**Depends on:** M3-REL-02
**Suggested branch:** `release/m3-rel-05-tag-the-release-candidate`

### Context
Exact tag/commit is the assessed baseline; no changes after submission.

### Tasks
- [ ] Confirm CI green on `main`, registers updated, PED/RTM consistent
- [ ] Create the tag via a PR-approved release process
- [ ] Record the tag in the submission block

### Done when
- [ ] Tag exists and the demo runs from it
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [M3-REL-06] Acceptance validation across in-scope requirements

Labels: `type:test`, `owner:P1`, `area:D`, `phase:6`, `priority:medium`

**Owner:** P1  |  **Phase:** 6  |  **Priority:** medium  |  **Rubric area:** D: Black-box / integration / system
**Trace:** Brief s15 'Acceptance' row
**Depends on:** M3-RTM-02, M3-RTM-03, M3-RTM-04
**Suggested branch:** `test/m3-rel-06-acceptance-validation-across-in-scope`

### Context
For every in-scope requirement, show the agreed behaviour is demonstrated and where the evidence is.

### Tasks
- [ ] Walk the RTM and mark missing acceptance evidence
- [ ] Resolve or record each gap as a limitation

### Done when
- [ ] No in-scope requirement without evidence or a stated limitation
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [M3-AI-01] AI Usage Register: log and verify AI use (P1)

Labels: `type:process`, `owner:P1`, `area:B`, `phase:6`, `priority:medium`

**Owner:** P1  |  **Phase:** 6  |  **Priority:** medium  |  **Rubric area:** B: Construction and configuration
**Trace:** Brief s8 AI accountability
**Suggested branch:** `chore/m3-ai-01-ai-usage-register-log-and`

### Context
Each member logs their own AI use and how it was verified (and anything changed or rejected).

### Tasks
- [ ] Add an entry for each AI-assisted artefact/test/code
- [ ] Record what was checked, run or rewritten

### Done when
- [ ] Register up to date before tagging; every AI-assisted test can be explained by its author
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [M3-RISK-01] Risk Register update: quality/requirements (P1)

Labels: `type:risk`, `owner:P1`, `area:X`, `phase:6`, `priority:medium`

**Owner:** P1  |  **Phase:** 6  |  **Priority:** medium  |  **Rubric area:** Cross-cutting
**Trace:** Continue the existing Risk Register
**Suggested branch:** `docs/m3-risk-01-risk-register-update-quality-requirements`

### Context
Update risks in your area. Candidates: requirement-implementation drift; RTM assembled late; low-value tests counted as padding; acceptance evidence missing.

### Tasks
- [ ] Review existing risks (probability, impact, mitigation, status)
- [ ] Add new risks the build has exposed
- [ ] Close or re-rate those retired

### Done when
- [ ] Register updated and consistent with the PED
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [M3-DEFENCE-01] Individual defence preparation (P1)

Labels: `type:process`, `owner:P1`, `area:X`, `phase:8`, `priority:high`

**Owner:** P1  |  **Phase:** 8  |  **Priority:** high  |  **Rubric area:** Cross-cutting
**Trace:** Brief s17 | Expected assessor questions
**Suggested branch:** `chore/m3-defence-01-individual-defence-preparation`

### Context
Rehearse, including parts you did not build.

### Tasks
- [ ] Trace one requirement from need to AC to implementation to test to evidence (use one you did not implement)
- [ ] Explain one decision, the alternative and its consequence
- [ ] Interpret one result and its limitation
- [ ] Explain one realistic failure, its control and the residual risk
- [ ] Defend the readiness classification with evidence

### Done when
- [ ] Rehearsed against live artefacts (repo, CI, RTM, PED), not screenshots
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [M3-AI-02] AI Usage Register: log and verify AI use (P2)

Labels: `type:process`, `owner:P2`, `area:B`, `phase:6`, `priority:medium`

**Owner:** P2  |  **Phase:** 6  |  **Priority:** medium  |  **Rubric area:** B: Construction and configuration
**Trace:** Brief s8 AI accountability
**Suggested branch:** `chore/m3-ai-02-ai-usage-register-log-and`

### Context
Each member logs their own AI use and how it was verified (and anything changed or rejected).

### Tasks
- [ ] Add an entry for each AI-assisted artefact/test/code
- [ ] Record what was checked, run or rewritten

### Done when
- [ ] Register up to date before tagging; every AI-assisted test can be explained by its author
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [M3-RISK-02] Risk Register update: security/process (P2)

Labels: `type:risk`, `owner:P2`, `area:X`, `phase:6`, `priority:medium`

**Owner:** P2  |  **Phase:** 6  |  **Priority:** medium  |  **Rubric area:** Cross-cutting
**Trace:** Continue the existing Risk Register
**Suggested branch:** `docs/m3-risk-02-risk-register-update-security-process`

### Context
Update risks in your area. Candidates: CI slips and removes everyone's gate; migration snapshot conflicts; auth rework late; secrets in history.

### Tasks
- [ ] Review existing risks (probability, impact, mitigation, status)
- [ ] Add new risks the build has exposed
- [ ] Close or re-rate those retired

### Done when
- [ ] Register updated and consistent with the PED
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [M3-DEFENCE-02] Individual defence preparation (P2)

Labels: `type:process`, `owner:P2`, `area:X`, `phase:8`, `priority:high`

**Owner:** P2  |  **Phase:** 8  |  **Priority:** high  |  **Rubric area:** Cross-cutting
**Trace:** Brief s17 | Expected assessor questions
**Suggested branch:** `chore/m3-defence-02-individual-defence-preparation`

### Context
Rehearse, including parts you did not build.

### Tasks
- [ ] Trace one requirement from need to AC to implementation to test to evidence (use one you did not implement)
- [ ] Explain one decision, the alternative and its consequence
- [ ] Interpret one result and its limitation
- [ ] Explain one realistic failure, its control and the residual risk
- [ ] Defend the readiness classification with evidence

### Done when
- [ ] Rehearsed against live artefacts (repo, CI, RTM, PED), not screenshots
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [M3-AI-03] AI Usage Register: log and verify AI use (P3)

Labels: `type:process`, `owner:P3`, `area:B`, `phase:6`, `priority:medium`

**Owner:** P3  |  **Phase:** 6  |  **Priority:** medium  |  **Rubric area:** B: Construction and configuration
**Trace:** Brief s8 AI accountability
**Suggested branch:** `chore/m3-ai-03-ai-usage-register-log-and`

### Context
Each member logs their own AI use and how it was verified (and anything changed or rejected).

### Tasks
- [ ] Add an entry for each AI-assisted artefact/test/code
- [ ] Record what was checked, run or rewritten

### Done when
- [ ] Register up to date before tagging; every AI-assisted test can be explained by its author
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [M3-RISK-03] Risk Register update: release/operations (P3)

Labels: `type:risk`, `owner:P3`, `area:X`, `phase:6`, `priority:medium`

**Owner:** P3  |  **Phase:** 6  |  **Priority:** medium  |  **Rubric area:** Cross-cutting
**Trace:** Continue the existing Risk Register
**Suggested branch:** `docs/m3-risk-03-risk-register-update-release-operations`

### Context
Update risks in your area. Candidates: staging not representative; load result over-interpreted; release work end-loaded; rollback data loss.

### Tasks
- [ ] Review existing risks (probability, impact, mitigation, status)
- [ ] Add new risks the build has exposed
- [ ] Close or re-rate those retired

### Done when
- [ ] Register updated and consistent with the PED
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [M3-DEFENCE-03] Individual defence preparation (P3)

Labels: `type:process`, `owner:P3`, `area:X`, `phase:8`, `priority:high`

**Owner:** P3  |  **Phase:** 8  |  **Priority:** high  |  **Rubric area:** Cross-cutting
**Trace:** Brief s17 | Expected assessor questions
**Suggested branch:** `chore/m3-defence-03-individual-defence-preparation`

### Context
Rehearse, including parts you did not build.

### Tasks
- [ ] Trace one requirement from need to AC to implementation to test to evidence (use one you did not implement)
- [ ] Explain one decision, the alternative and its consequence
- [ ] Interpret one result and its limitation
- [ ] Explain one realistic failure, its control and the residual risk
- [ ] Defend the readiness classification with evidence

### Done when
- [ ] Rehearsed against live artefacts (repo, CI, RTM, PED), not screenshots
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---

### [M3-PRES-01] Presentation dry run from live artefacts

Labels: `type:process`, `owner:all`, `area:X`, `phase:8`, `priority:high`

**Owner:** all  |  **Phase:** 8  |  **Priority:** high  |  **Rubric area:** Cross-cutting
**Trace:** Brief s18
**Depends on:** M3-REL-05
**Suggested branch:** `chore/m3-pres-01-presentation-dry-run-from-live`

### Context
Suggested split (~25 min): P1 stages 1,2,5; P2 stages 3,4; P3 stages 6,7. Present the identified RC, not a different build.

### Tasks
- [ ] Run through with a timer
- [ ] Handover rehearsal
- [ ] Check every link and the staging URL work beforehand

### Done when
- [ ] Dry run completed with notes actioned
- [ ] PR links this issue, with two non-author approvals showing real review comments
- [ ] RTM / registers updated in the same PR where this touches a requirement, test or defect
- [ ] Any AI-assisted content logged in the AI Usage Register and verified

---
