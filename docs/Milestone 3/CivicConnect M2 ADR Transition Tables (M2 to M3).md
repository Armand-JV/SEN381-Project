# CivicConnect — M2 → M3 ADR Transition Tables

*Prepared for PED v3.0, Section 2 (M2 baseline conformance) and Section 3 (Controlled changes). Format follows the M3 brief, Area A: original decision → implementation evidence/problem → revised decision → consequence.*

## How to read this document

- **Source of the ADRs.** I did not have PED v2.0 or your Decision Log, so the six ADRs below are **inferred** from the repository (README, projects, controller, migrations, CI) and the M2 brief's required decision areas (architecture, persistence, technology, two design patterns, interface). Rename them to your real ADR IDs.
- **Source of requirements.** The Milestone 1 PDF is the milestone brief, not your approved requirements baseline, so it contains no FR/NFR text. Requirement references use the IDs and slice boundaries from your M3 work split (FR-001–006 Requester, FR-007–013 Staff, FR-014–016 Management, NFR-017–020 security/audit, NFR-024 three-second target). Please confirm the wording against your PED.
- **Revised decisions are proposals.** Nothing here claims the team has already changed a decision. Each "Revised decision" row is a recommendation for the team to accept, amend or reject, and the Status row says so.
- **Evidence limits.** Findings come from the files shared. Anything I could not confirm by running the app is marked **verify**.
- **Scope rule from the M3 brief.** A short conformance table plus 1–3 important ADR/change records is normally enough. The summary below recommends which transitions deserve a full record and which stay as table rows.

## Summary: M2 baseline → M3 implementation

| ID | M2 decision (inferred) | Conformance in code | Transition type | Record level | Suggested owner |
| --- | --- | --- | --- | --- | --- |
| ADR-01 | Layered architecture: Domain / Application / Web | Partial — repository abstractions unused, logic in controller, patterns not wired into DI | Deviation (significant) | **Full ADR/change record** | P1 (conformance), P2 for DI/wiring |
| ADR-02 | PostgreSQL + EF Core, code-first migrations | Conformant on schema; integrity gaps found | Refinement (significant) | **Full ADR/change record** | P1 (Requester data), P3 for perf evidence |
| ADR-03 | .NET 10 / ASP.NET Core / Swagger / xUnit stack, GitHub Actions | Conformant on versions; config, tooling and CI gaps | Refinement | Conformance row + tasks | P2 |
| ADR-04 | Observer for request state changes | Interface and notifier exist; no concrete observer, not registered | Incomplete application | Conformance row + tasks (upgrade to ADR if dropped) | P3 (audit trail) |
| ADR-05 | Strategy for category-specific validation | Interface and context exist; no concrete strategy, not registered | Incomplete application | Conformance row + tasks (upgrade to ADR if dropped) | P1 |
| ADR-06 | REST API via ASP.NET Core controllers | Three endpoints; simulated identity, no auth, entity returned directly | Deviation from security intent (significant) | **Full ADR/change record** | P2 (RBAC) |

**Recommended full records (max three):** ADR-01, ADR-02 and ADR-06. ADR-03, ADR-04 and ADR-05 stay as conformance rows unless the team decides to drop a pattern or change the stack, in which case that becomes a superseding ADR.

---

## ADR-01 — Layered architecture (Domain / Application / Web)

| Stage | Content |
| --- | --- |
| **Original decision (M2)** | Three modules with a one-way dependency: `CivicConnect.Domain` (entities, enums), `CivicConnect.Application` (business logic, interfaces, repositories, Observer and Strategy patterns), `CivicConnect.Web` (controllers, DbContext, migrations). Tests in `CivicConnect.Tests`. Source: README "Project Structure". |
| **Implementation evidence** | Solution contains the four projects. Dependency direction is correct (Web → Application → Domain). `IRequestRepository` and `ICategoryRepository` are declared in Application. `CivicConnectDbContext` is in `Web/Data`. |
| **Problem / deviation found** | (1) No implementation of either repository interface exists in the files shared, and nothing uses them. (2) `RequestsController` injects `CivicConnectDbContext` directly and holds the business logic: validation call, state change, history writing, notification. (3) There is no Application service layer between controller and persistence. (4) `Program.cs` registers only the DbContext, Swagger and controllers. `CategoryValidationContext` and `RequestStateNotifier` are not registered, so the controller's constructor dependencies cannot be resolved — **verify** by calling `POST /api/requests`. |
| **Revised decision (proposed)** | **Option A (recommended):** add a `RequestService` in Application that depends on `IRequestRepository`, implement `EfRequestRepository` in Web/Data, register everything in DI, and keep controllers thin. **Option B:** formally accept DbContext-in-controller as a deliberate simplification, delete the unused repository interfaces, and record the loss of testability as technical debt. Option A fits the Moq-based unit-test approach already in the test project and the three-slice split. |
| **Consequence** | RTM: re-point implementation evidence from the controller to the service/repository for FR-001–006 and the Staff and Management slices. Tests: service unit tests can mock `IRequestRepository` without a database. PED: Section 2 row changes from "Deviation" to "Resolved by ADR-01 change"; Section 3 gets the change record. Risks: close or update any layering-erosion risk. If Option B is chosen, add a Technical Debt Register entry. |
| **Status** | Proposed — team to confirm option and record approval. |

---

## ADR-02 — Persistence: PostgreSQL + EF Core (code-first migrations)

| Stage | Content |
| --- | --- |
| **Original decision (M2)** | PostgreSQL accessed through Entity Framework Core with Npgsql. Entities: Request, Category, Comment, StatusHistory. Schema produced by migrations. Categories seeded. Source: README "Technology Stack", "Persistence". |
| **Implementation evidence** | Migration `20260930160638_InitialCreate` with Designer and snapshot; `db_schema.sql` script; `HasData` seeding in `CivicConnectDbContext`; identity keys; `timestamp with time zone` columns; foreign-key indexes on `CategoryId` and `RequestId`. EF Core 10.0.12, Npgsql provider 10.0.3. The schema matches the entity model. |
| **Problem / deviation found** | (1) `Requests → Categories` uses **cascade delete**, so deleting a category would silently delete every request and its history. (2) No concurrency token, so two staff updating one request is last-write-wins. (3) `RequesterId`, `AssigneeId`, `CreatedBy`, `ChangedBy` are free text with no user table; acceptable only until real authentication exists. (4) No indexes on likely filter columns (`State`, `RequesterId`, `AssigneeId`, `CreatedAt`); whether that matters against the three-second target (NFR-024) is not yet evidenced. (5) The initial history row is created with `RequestId = request.Id` before the request is saved; linking through the `History` navigation is safer — **verify** with an integration test. (6) Tests reference the EF InMemory provider, which does not enforce foreign keys or relational constraints, so it cannot prove the integrity behaviour in (1) and (5). (7) `Id` is a database identity; if FR-003 (unique ID) means a user-facing reference, none exists yet. |
| **Revised decision (proposed)** | Add a **new** migration (do not edit the baselined `InitialCreate`) that changes `Requests → Categories` to `Restrict`. Add a PostgreSQL concurrency token (xmin-based) on `Request`. Add indexes only after the performance test shows a need. Run integration tests against real PostgreSQL (for example a GitHub Actions service container). Confirm the meaning of FR-003 and add a reference column only if required. |
| **Consequence** | New migration and regenerated `db_schema.sql`. RTM data/persistence-impact column updated for FR-001–006 and the audit requirement. Defect Register entry if the cascade is judged a defect. Risk Register: update data-integrity risk. The load test (P3) supplies the evidence for or against indexing. PED Section 3 records the change; the original decision stays visible as superseded. |
| **Status** | Proposed — team to confirm; migration and tests not yet written. |

---

## ADR-03 — Technology stack and tooling

| Stage | Content |
| --- | --- |
| **Original decision (M2)** | .NET 10 with ASP.NET Core (MVC/API), PostgreSQL 18, EF Core, Swagger for API documentation, xUnit with Moq and FluentAssertions for tests, GitHub Actions for early CI. Source: README, project files. |
| **Implementation evidence** | All four projects target `net10.0`. Packages are pinned (EF Core 10.0.12, Npgsql 10.0.3, Swashbuckle 10.2.3, xUnit 2.9.3, Moq 4.21.0, FluentAssertions 8.11.0). `.github/workflows/test.yml` builds Domain, Application and Web in Release and runs the test project on push and pull request to `main`. |
| **Problem / deviation found** | (1) `appsettings.json` commits a database connection string with credentials (a development default). This conflicts with the "no committed secrets" rule and gives staging no clean override path. (2) `AddControllersWithViews()` is configured but there are no views, no `Home` controller, and a default route to `Home/Index`; the frontend decision is not visible in the repo. (3) Swagger is enabled only in Development, so assessors cannot see API documentation on staging unless that is decided deliberately. (4) FluentAssertions 8.x moved to a different licence; **verify** the terms against the official licence for the cost/licensing criterion, or switch to xUnit assertions. (5) The only test is `UnitTest1.Test1` with an empty body, which fails the Meaningful Test Rule. (6) CI runs build and test only: no static analysis, no dependency or secret scanning, no database service, and no check that is demonstrated to block a merge. |
| **Revised decision (proposed)** | Supply the connection string through an environment variable (`ConnectionStrings__DefaultConnection`) or user-secrets, with a non-secret placeholder in the repo. Decide explicitly between a Razor/MVC front end and an API-only host, and align `Program.cs` (`AddControllers` if API-only). Decide Swagger exposure per environment. Settle the FluentAssertions question. Delete the placeholder test. Extend CI with analyzers or `dotnet format --verify-no-changes`, a dependency/secret scan, and a PostgreSQL service for integration tests; make the build/test step a required status check on `main`. |
| **Consequence** | PED Section 4 (configuration/secrets) and Section 5 (CI and gates) are populated from this. RTM technology-decision column stays valid for versions. Risk Register: update security and dependency/licensing risks. Staging configuration (P3) depends on the secrets approach being settled first. |
| **Status** | Proposed — conformance row plus tasks; escalate to an ADR only if the stack itself changes. |

---

## ADR-04 — Observer pattern for request state changes

| Stage | Content |
| --- | --- |
| **Original decision (M2)** | Design problem: several parts of the system must react when a request changes state without the state-change code knowing about them. Decision: Observer, with `IRequestStateObserver` and `RequestStateNotifier`. Source: README "Initial Design Patterns". |
| **Implementation evidence** | `IRequestStateObserver.OnStateChangedAsync(request, oldState, newState)`; `RequestStateNotifier` iterates an injected `IEnumerable<IRequestStateObserver>`; invoked from `RequestsController.UpdateState` after `SaveChangesAsync`. |
| **Problem / deviation found** | (1) No concrete observer exists in the files shared, so the pattern has no behaviour to verify yet. (2) The notifier is not registered in DI (see ADR-01). (3) Observers run sequentially after the commit with no failure isolation: an exception makes the endpoint return an error although the state change is already saved. (4) Only `UpdateState` notifies; initial submission does not. Whether the requester must be told on submission is a requirements question. (5) `UpdateState` accepts any target state with no transition rules (for example `Closed → Open`) and no check that an assignee exists for `Assigned`. No M2 ADR covers lifecycle rules, yet black-box state-transition testing depends on them. |
| **Revised decision (proposed)** | Keep Observer only if at least one real observer is built. Candidates tied to requirements: a notification/feedback observer for the Requester slice and an audit-trail observer for NFR-020. Register observers and the notifier in DI. Wrap each observer call so one failure is logged and does not stop the others, keeping the after-commit semantics. Move the call into the application service (ADR-01). Raise a **new ADR for request lifecycle rules** defining allowed transitions. If no genuine observer can be justified, record the pattern as dropped and supersede the M2 decision rather than leaving dead scaffolding. |
| **Consequence** | Unit tests with Moq: observer receives correct old/new state; a failing observer does not block the rest. Integration test: state update persists history and triggers the observer. RTM: link design-decision column to the status and audit requirements. PED: complexity trade-off (extra indirection, ordering and failure handling) must be stated honestly. |
| **Status** | Proposed — incomplete application of an M2 decision; team to choose build or drop. |

---

## ADR-05 — Strategy pattern for category-specific validation

| Stage | Content |
| --- | --- |
| **Original decision (M2)** | Design problem: validation rules differ by request category and should be extensible without editing a growing conditional. Decision: Strategy, with `ICategoryValidationStrategy` (keyed by `CategoryId`) and `CategoryValidationContext`. Source: README. |
| **Implementation evidence** | Interface and context exist. The context selects the first strategy whose `CategoryId` matches, otherwise falls back to a default rule (non-empty title and description). Called in `RequestsController.CreateRequest` after the category-exists check. The context class lives in `StrategyContext.cs` (file/class name mismatch). |
| **Problem / deviation found** | (1) No concrete strategy exists in the files shared; the only live logic is the default fallback, so the pattern is not yet demonstrably justified. (2) `Validate` returns `bool`, so the API can only answer "validation failed" with no reason, which weakens user feedback. (3) Strategies are not registered in DI. (4) Strategies key off category IDs 1–3, which are seed data, coupling behaviour to seeded identity values. (5) Several strategies for one `CategoryId` silently resolve to the first. (6) The DTO has no required/length attributes, so basic validation is not separated from category rules. |
| **Revised decision (proposed)** | First confirm from the M1 acceptance criteria which rules genuinely differ between Infrastructure, Utilities and Parks; do not invent differences. If they differ, implement one concrete strategy per category, change the return type to a result with error messages, register all strategies in DI, and fail fast on duplicate `CategoryId` registration. If they do not differ, drop the pattern and record a superseding decision. |
| **Consequence** | Black-box tests (P1): equivalence partitioning and boundary values per category rule, with expected results written before execution. RTM design-decision column for FR-001 and FR-002 points to the concrete strategies. PED trade-off: more classes and registration discipline in exchange for per-category extensibility. |
| **Status** | Proposed — incomplete application; decision depends on the real category rules. |

---

## ADR-06 — Interface decision: REST API via ASP.NET Core controllers

| Stage | Content |
| --- | --- |
| **Original decision (M2)** | Expose functionality through an ASP.NET Core controller-based HTTP API documented with Swagger, as the interaction boundary between client and system. Source: README "API Documentation", `RequestsController`. |
| **Implementation evidence** | Three endpoints: `POST /api/requests`, `GET /api/requests/{id}`, `PUT /api/requests/{id}/state`. DTOs `CreateRequestDto` and `UpdateStateDto` are defined in the controller file. Swagger UI in Development. |
| **Problem / deviation found** | (1) Identity is simulated: `RequesterId = "user123"` and `ChangedBy = "admin"`. `UseAuthorization()` is called but no authentication scheme or `[Authorize]` attribute exists, so any caller can change any request's state. This does not yet meet the RBAC intent (NFR-017–019). (2) The `Request` domain entity is returned directly, exposing the internal model and navigation collections. (3) No validation attributes on DTOs; `UpdateStateDto.NewState` may accept enum values outside the defined set — **verify** with value 99. (4) Errors are plain strings rather than a consistent problem-details format. (5) No list, filter, assign, comment or history endpoints yet, so most slice requirements have no API path. (6) `GET` has no ownership check, so one requester could read another's request. |
| **Revised decision (proposed)** | Introduce authentication and role-based policies (mechanism to be chosen by the team and recorded; keep it proportionate to the schedule). Replace the hard-coded identities with the authenticated user. Add response DTOs, DTO validation including `Enum.IsDefined`, and consistent `ProblemDetails` errors. Add endpoints required by each slice. Keep Swagger decision aligned with ADR-03. |
| **Consequence** | Negative/unauthorised tests (minimum three): requester attempting a staff action, staff accessing an out-of-scope request, unauthenticated call. API/integration tests (minimum three, including failure cases). RTM: security and interface columns for NFR-017–020. Risk Register: security risk moves from "accepted simulation" to a mitigated or open item. PED Section 3 records the change; the original simulated-identity approach is preserved as the M2 position. |
| **Status** | Proposed — highest-priority transition because it affects security, tests and the readiness classification. |

---

## Candidate new ADR (not in M2)

**Request lifecycle / state-transition rules.** The `RequestState` enum has seven states (Open, Assigned, InProgress, Resolved, Closed, Cancelled, Rejected) and `UpdateState` currently allows any move between them. A short ADR defining permitted transitions, who may perform each, and which require a reason would give the state-transition black-box tests their expected results and give ADR-04 a concrete trigger. I can draft that transition matrix next if useful.

## Suggested next steps

1. Map ADR-01 to ADR-06 to your real Decision Log IDs and correct anything I inferred wrongly.
2. Decide Option A or B for ADR-01 first; ADR-04, ADR-05 and ADR-06 depend on it.
3. Confirm the real category rules and the FR-003 meaning before building ADR-05 or the reference column.
4. Write the three full change records (ADR-01, ADR-02, ADR-06) in the controlled ADR location, and link them from PED v3.0 Sections 2 and 3 rather than copying the text.
5. Update the RTM in the same pull requests that implement each change.
