# P3 Decisions (M3 - Release Candidate)

Owner: Person 3 (Release, Performance & Operations).
Purpose: record every P3 decision with rationale, alternatives, and M2 comparison. Each issue gets a new `##` heading.

---

## M3-UI-01 (Issue #21) — API-only demo path, no Razor UI

- **Date:** 2026-10-14
- **Issue:** [#21 — Decide and build minimal UI (or documented API-only demo path)](https://github.com/Armand-JV/SEN381-Project/issues/21)
- **Decided by:** P3 (user decision, asked explicitly per workflow)
- **Status:** Implemented on `feature/m3-p3-phase2` via cherry-pick `e9a6b08` (ex-`feature/m3-ui-01` commit `6fead70`); part of combined P3 Phase 2 PR (pending PR)

### Context
- `src/CivicConnect.Web/Program.cs` registers MVC (`AddControllersWithViews`) and maps default route `{controller=Home}/{action=Index}` (Program.cs:7,36-39), plus `/Home/Error` for error handling.
- Repo has no `HomeController`, no `Views/`, and `src/CivicConnect.Web/wwwroot/` contains only `.keep`, so `/` returns 404.
- Only real controller is `src/CivicConnect.Web/Controllers/RequestsController.cs` (`api/requests`). E2E journeys TC-S-01/TC-S-02 need a driver.
- Auth/RBAC (P2, NFR-017/018) not yet implemented; controller still uses hardcoded `user123`/`admin`.

### Decision
**Go API-only.** No Razor/static UI for M3. E2E is driven via `HttpClient` against the API, demoed via Swagger UI.

### Alternatives considered
1. **Minimal Razor UI** (submit form, my-requests list, staff queue, status update) — rejected: larger scope, blocked on P2 auth, duplicates API work, would need a browser E2E harness.
2. **API-only** — chosen: smallest change that unblocks TC-S-01/02, matches current API reality, keeps P3 end-load (staging/perf/release) on track.

### Consequences / implementation notes
- Removed the unused default `Home/Index` route (`MapControllerRoute`) so `/` no longer 404s; added `MapControllers()` for attribute-routed APIs.
- Added `GET /`: redirects to `/swagger` in Development, returns `{ service, health, demo }` pointer otherwise.
- Changed `UseExceptionHandler("/Home/Error")` to `UseExceptionHandler()` (no missing Home controller; returns ProblemDetails).
- Kept Swagger as the documented demo path; E2E tests use `HttpClient`, not a browser driver.
- No `HomeController`/`Views` to maintain.

### Demo path (API-only)
- Development: run API, open `/swagger` (root `/` redirects there), exercise `POST /api/requests`, `GET /api/requests/{id}`, `PUT /api/requests/{id}/state`.
- Non-development / staging: `GET /health` proves the deployment; `GET /` points to `/health`.
- E2E driver: `HttpClient` (TC-S-01/02), no browser needed.

### M2 comparison
- M2 baseline (PED v2.0, `docs/Milestone 2/CivicConnect_PED_v2.pdf`): frontend = **React 19** component UI (service-request forms, status views, filtering, history, management reports) talking to backend over HTTP APIs; backend = ASP.NET Core MVC/C# + PostgreSQL; contract = resource-oriented JSON API `POST/GET /api/v1/...` with OpenAPI description (ADR-001 React frontend, ADR-002 backend, ADR-007 API). Server-rendered form posts listed only as a conditional alternative ("valid if Person 2 selects a server-rendered stack"). NFR-021 usability explicitly "satisfied in the presentation layer".
- Current `main` has **neither**: no React app, no Razor views, no `HomeController` — only `AddControllersWithViews` + default `Home/Index` route that 404s. So M2 frontend is not implemented.
- This decision (API-only for M3 demo/E2E, no UI build) is therefore a **controlled scope deferral/deviation from M2**: frontend deferred, E2E/demo runs API-only via `HttpClient` + Swagger. Flag for P1 conformance table (M3-CONF-01): significant deviation, link here.

### Trace
- Unblocks: M3-TEST-13 (TC-S-01), M3-TEST-14 (TC-S-02), M3-TEST-15 (staging run).
- Related defects: DEF-001 (DI), DEF-004 (hardcoded identities) — E2E still depends on P2 fixes.

---

## P3 Phase 2 batch strategy (Issues #21 + #32 + #35 + #36) — one PR

- **Date:** 2026-10-14
- **Decided by:** P3 (user decision, asked explicitly per workflow)
- **Status:** Branch ready, implementation pending

### Decisions
1. **Branch:** new `feature/m3-p3-phase2` from `origin/main`, cherry-picked #21 (`e9a6b08`). Old `feature/m3-ui-01-*` left unpushed. One PR will close all four issues.
2. **Manager role:** `Admin` role = Manager for M3 (no separate Manager role/policy). Matches current `AccessControlService` (Admin bypass) and JWT/test users from #120.
3. **Migrations:** single EF migration containing both the new `AuditLog` table (#35) and all dashboard/queue indexes (#36).

### Order
1. #36 + #35 model change (entity + `OnModelCreating` indexes) → one `dotnet ef migrations add` → regenerate `db_schema.sql`.
2. #35 audit writes + manager-only read endpoint.
3. #32 dashboard aggregation endpoint (shaped to use the new indexes).

### Merge of main (2026-10-14, `edc4f62` FR-007..010 + `b21e0fc` FR-012 observers)
- Conflicts resolved in 4 files (the M3-GOV-04 scenario): kept BOTH sides everywhere —
  their staff endpoints/transition policy/seeds/`AuditEntries` plus our `AuditLogs`/indexes/dashboard.
- `UpdateState`: took their `GetActor()` (throws when unauthenticated — better for audit integrity than our `"Anonymous"` fallback); our `AuditLogs` write kept below their policy guards.
- Their merge duplicated the JWT registration in `Program.cs` (`Scheme already exists: Bearer` crashed EVERY factory test); removed the stale pre-#121 block, kept their unified policy-scheme + our `AddProblemDetails()`. Also deduped double `MapControllers()`/registrations.
- Test fallout is theirs, not ours: `AuditLogTests` needed `AssigneeId` set (their new Assigned guard) — fixed. 3 remaining failures are their `RequestControllerTests` hitting the new `STAFF,MANAGER`-only `UpdateState` with requester tokens (their tests not updated for their own authz change). Our 5 tests pass; suite went from all-factory-failing to 56/59.
- Open team items (unchanged): one-table audit question (`AuditEntries` is now IN main), role-case convention (main mixes `STAFF` attributes with `Admin` checks), shared-DB migration apply.

---

## FR-014 (Issue #32) — Management dashboard counts

- **Date:** 2026-10-14
- **Issue:** [#32](https://github.com/Armand-JV/SEN381-Project/issues/32)
- **Decided by:** P3 (user decisions, asked explicitly per workflow)
- **Status:** Implemented on `feature/m3-p3-phase2` (pending build verify)

### Context
- No dashboard endpoint exists. Issue asks for aggregation (counts by state, category, age bucket) with filters, Manager-only, query shaped to use #36 indexes, perf-sensitive vs NFR-024 3s target.
- Depends on NFR-018 (closed #120) and M3-DATA-01 (#36).

### Decisions (user, 2026-10-14)
- **Access:** `Admin` OR `Manager` role (code checks `IsInRole("Admin")`, tests model `Manager`; accept both).
- **Age buckets:** deferred (ship `total` + `byState` + `byCategory` now; buckets later with perf work).
- **Endpoint:** `GET api/reports/requests-summary?state&categoryId&assigneeId&from&to` (`from`/`to` filter `CreatedAt`).
- **Keys:** `byState` uses enum names (`Open`, `Assigned`, …), `byCategory` uses integer category IDs.
- **RTM:** title still "working title" (no RTM file in repo; P1 owns RTM). Built to issue text; confirm against RTM in M3-RTM-04.

### Verification (TC-U-06, 2026-10-14; re-verified live post-merge 2026-10-14)
- `DashboardSummaryTests` (3 tests): counts reconcile, filters combine, non-manager gets `Forbid` — all pass (`dotnet test --filter DashboardSummaryTests`: 3/3).
- Live (remote DB): Admin JWT → 200 `{"total":7,"byState":{"Closed":1,"Open":6},"byCategory":{"1":7}}`; Requester → 403; anonymous → 401.
- Post-merge: full journey live — MANAGER assign (200) → state to InProgress (200) → audit read shows the entry → dashboard `{"total":7,"byState":{"Open":5,"InProgress":1,"Closed":1}}` reconciles.
- Roles aligned to main's UPPERCASE convention (`ADMIN`/`MANAGER`) after merge #123; live reads re-verified with `?role=MANAGER`.
- Incidental fix: added `AddProblemDetails()` — required by `UseExceptionHandler()` (no path) on .NET 10; the test host (env `Testing`) hits the non-Development branch.
- Pre-existing (not mine, not fixed): `TestWebApplicationFactory` is broken on main (Npgsql vs InMemory provider conflict — `RequestControllerTests` fails identically); TC-U-06 deliberately avoids it. Harness fix belongs to P2.

---

## NFR-020 (Issue #35) — Audit trail

- **Date:** 2026-10-14
- **Issue:** [#35](https://github.com/Armand-JV/SEN381-Project/issues/35)
- **Decided by:** P3 (user decisions, asked explicitly per workflow)
- **Status:** Implemented on `feature/m3-p3-phase2` (tests green; migration NOT applied to shared DB — awaiting user call)

### Context
- `StatusHistory` is requester-facing; issue asks for separate append-only `AuditLog` (actor, action, entity type/id, before/after summary, timestamp, correlation id) + migration, writes on state changes/assignment/role-protected actions, Manager-only read, no update/delete path.
- M2 ADR-004/005: audit written in same transaction, synchronous Observer.

### Decisions (user, 2026-10-14)
- Manager read = `Admin` OR `Manager` role (same rule as #32).
- Write path = direct `DbContext` write before `SaveChanges` (same transaction, ADR-004). Observer path rejected (runs after commit, DEF-006).
- Table ships in the single combined migration with #36 indexes.
- v1 triggers: state changes now; assignment wiring deferred — no assign endpoint exists yet (P2 FR-008 pending). Hook it up when FR-008 lands.

### Implementation
- `CivicConnect.Domain/Entities/AuditLog.cs` (new): `Actor, Action, EntityType, EntityId, BeforeSummary, AfterSummary, Timestamp, CorrelationId`.
- `RequestsController.UpdateState`: appends `Action="StateChanged"` entry (before/after state names, fresh `CorrelationId`) in the same `SaveChanges`.
- `Controllers/AuditController.cs` (new): `GET api/audit?entityType&entityId`, Admin/Manager only, `Take(200)`, newest first. No POST/PUT/DELETE exists.

### Verification (2026-10-14)
- `AuditLogTests` (2 tests): state change → exactly one entry with actor/action/before/after/correlation; manager reads, requester gets `Forbid`. `dotnet test --filter "AuditLogTests|DashboardSummaryTests"`: 5/5 pass.
- Migration `20261014131923_M3P3_AuditLog_And_QueryIndexes`: 1 `CreateTable` + 8 `CreateIndex`; `has-pending-model-changes` clean; `db_schema.sql` regenerated idempotent (75 → 187 lines).
- Live against shared DB: NOT run — table doesn't exist there until `dotnet ef database update` is applied (shared-state change, needs user go-ahead).

---

## M3-DATA-01 (Issue #36) — Indexes for dashboard/queue queries

- **Date:** 2026-10-14
- **Issue:** [#36](https://github.com/Armand-JV/SEN381-Project/issues/36)
- **Decided by:** P3 (pending index list)
- **Status:** Decided to build; index list pending

### Context
- Model only has FK/PK indexes. Dashboard/queue filters hit `State`, `RequesterId`, `AssigneeId`, `CreatedAt` (`Request.cs`, `CivicConnectDbContext.cs`).
- Must follow migration rule M3-GOV-04 (#74, closed) and regenerate `db_schema.sql`.

### Pending decisions (to ask)
- Exact index set (single-column vs composite, e.g. `(State, CategoryId)`, `(AssigneeId, State)`, `(RequesterId, CreatedAt)`).
- Before/after query evidence capture for M3-PERF.
