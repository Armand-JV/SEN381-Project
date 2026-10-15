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

### Addendum 2026-10-15: `/` yielded to the React SPA (user decision)
- Premise changed: PR #134 merged a React Tier-1 SPA served from `wwwroot` with SPA fallback, so "no UI exists" is no longer true.
- Change: removed our `GET /` → Swagger redirect; `/` now falls through to the SPA (`MapFallbackToFile`). Swagger stays the API demo path at `/swagger`; API identification at `/api/version`; E2E stays `HttpClient`-driven (TC-S-01 unaffected).
- M2 comparison flips back in our favor: a React frontend was the M2 baseline all along (ADR-001), so the deferral deviation is retired for the Tier-1 pages.

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
- **Status:** Implemented, migrated and live-verified on the shared DB 2026-10-14
  (branch `feature/m3-p3-phase2`, merged to `main` as PR #129)

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

### Verification (2026-10-14, applied + live-verified same day)
- `AuditLogTests` (2 tests): state change → exactly one entry with actor/action/before/after/correlation; manager reads, requester gets `Forbid`. `dotnet test --filter "AuditLogTests|DashboardSummaryTests"`: 5/5 pass.
- Migration `M3P3AuditLogAndQueryIndexes` (class renamed CA1707-compliant): 1 `CreateTable` + 8 `CreateIndex`; `has-pending-model-changes` clean; `db_schema.sql` regenerated idempotent.
- Shared DB: `SeedRequestTwo` baselined as applied (PKs occupied by live rows, live data kept); our migration applied clean. Live (MANAGER JWT): assign 200, state change 200, audit read returns exactly the new `StateChanged` row.

---

## M3-DATA-01 (Issue #36) — Indexes for dashboard/queue queries

- **Date:** 2026-10-14
- **Issue:** [#36](https://github.com/Armand-JV/SEN381-Project/issues/36)
- **Decided by:** P3 (user decisions, 2026-10-14)
- **Status:** Implemented, migrated and live-verified (merged to `main` as PR #129)

### Context
- Model only has FK/PK indexes. Dashboard/queue filters hit `State`, `RequesterId`, `AssigneeId`, `CreatedAt` (`Request.cs`, `CivicConnectDbContext.cs`).
- Must follow migration rule M3-GOV-04 (#74, closed) and regenerate `db_schema.sql`.

<<<<<<< HEAD
### Decisions (user, 2026-10-14)
- Index set: 4 singles (`State`, `RequesterId`, `AssigneeId`, `CreatedAt`) + 2 composites (`State,CategoryId`, `AssigneeId,State`), plus 2 audit read-path indexes (`EntityType,EntityId`, `Timestamp`).
- Single combined migration with #35 (`M3P3AuditLogAndQueryIndexes`).

### Verification
- `has-pending-model-changes` clean; applied to shared DB 2026-10-14; all 8 new indexes confirmed in `pg_indexes`.
- Before/after query evidence: deferred to M3-PERF-03 execution.

---

## P3 Phase 3 batch (Issues #42, #101, #61, #75) - one PR

- **Date:** 2026-10-14
- **Decided by:** P3 (user decision)
- Branch `feature/m3-p3-phase3` from current `main`; one PR on completion.
  #33 (FR-015) and #34 (FR-016) excluded: no acceptance criteria in the RTM (P1 owns it).

---

## M3-PERF-01 (Issue #61) - Performance operation, target and workload

- **Date:** 2026-10-14
- **Issue:** [#61](https://github.com/Armand-JV/SEN381-Project/issues/61)
- **Decided by:** P3 (user decisions, asked explicitly per workflow)
- **Status:** Plan recorded; execution in M3-PERF-03

### Decisions (user, 2026-10-14)
- **Operation:** `GET /api/reports/requests-summary` (filtered + unfiltered). Rationale: FR-014 names it performance-sensitive; NFR-024 (clarified by CR-001: at least 95% of regular operations within 3s under expected load) applies directly; the fresh #36 indexes give a before/after story.
- **Tool:** NBomber (.NET-native via NuGet, version-pinned, CI-runnable; no extra installs).
- **Workload:** small - 10 virtual users, 2 minutes steady (plus warm-up excluded), 1,000 seeded requests across categories/states. Detailed seed set built in M3-PERF-02.
- **Target:** p95 response time within 3s, zero errors; anything else fails.
- **Environment:** local app + local/disposable PostgreSQL. Never the shared remote DB.

### Why small and local (user's words: for reliability and scale)
- Reliability: controlled, repeatable runs; same data, same host, comparable numbers across runs (including the before/after index comparison).
- Scale honesty: a small local workload cannot disrupt the team's shared database or anyone's demo, and matches the M2 single-instance assumption - with the limitation stated plainly (see below).

### What this does NOT prove
- Production behavior (different host, data volume, network).
- Concurrent-write contention (read-only workload).
- Behavior beyond 10 users / 1k rows (that is M3-PERF-03 interpretation territory, not this plan).

### Execution TC-P-01 (M3-PERF-03 #63, run 2026-10-15)
- Environment: local app + isolated local PostgreSQL 5433 (throwaway cluster, Armand's
  dump restored + `seed-perf.sql` 1,000 rows). Shared DB untouched throughout.
- Result (10 VUs x 2 min, 109,525 requests, 0 errors, ~913 RPS):
  p50 9.45ms, p75 14.27ms, **p95 23.71ms**, p99 32.9ms, max 486ms.
- Vs target: p95 23.71ms << 3,000ms with zero errors — **PASS** by two orders of magnitude.
- What this does NOT prove (PED s8 limits): production behavior; write contention;
  beyond-10-user scale; cold-start/JIT warm-up excluded by steady-state measurement.
  Note: NBomber free license is personal-use-only messaging appeared at runtime —
  academic use here, but flag before any organizational/commercial use.

---

## M3-STG-01 (Issue #75) - Staging approach

- **Date:** 2026-10-14
- **Issue:** [#75](https://github.com/Armand-JV/SEN381-Project/issues/75)
- **Decided by:** P3 (user decisions, asked explicitly per workflow)
- **Status:** Decision recorded; build-out in M3-STG-02..06 (phase 5)

### Comparison (user-reviewed)
- **Compose-based local (chosen):** Docker compose with web + PostgreSQL. Reproducible,
  version-pinned, runnable by the assessor; matches the M2 single-deployable assumption
  and M1 free-tier constraint. Dockerfile/compose land in M3-STG-03.
- **Local simulated without containers (interim):** `dotnet run` + local PostgreSQL 18.
  Zero new tooling; proven working end-to-end repeatedly this session
  (build, JWT auth, seeded and live-DB runs). Serves as staging until compose lands.
- **Hosted cloud (rejected):** no account, pipeline, or budget in place; operational
  cost disproportionate to a 3-person team on this timeline. No bonus marks for
  infrastructure complexity (per the issue).

### Decision (user, 2026-10-14)
- Target: compose-based local staging; interim: local `dotnet run` + PostgreSQL.
- Shared remote DB (`armanddatabase.webhop.me`) remains the team data tier in the
  interim; staging Postgres (compose or local) is seeded separately - never load-test
  or experiment against shared data (ties M3-PERF-01 environment rule).

### Assessor demo path (proven 2026-10-14, updated for SPA yield 2026-10-15)
1. Clone repo at the tagged release candidate, restore, `dotnet run` (http profile).
2. Open `/` for the React SPA, `/swagger/index.html` for the API.
3. Mint role token: `GET /api/testauth/token/{user}?role=ADMIN|MANAGER|STAFF`.
4. Authorize in Swagger (Bearer), exercise submit → assign → state → history →
   dashboard → audit journeys.
5. `GET /health` proves DB connectivity.

---

## M3-STG-03 (Issue #77) - Dockerfile and compose

- **Date:** 2026-10-15
- **Issue:** [#77](https://github.com/Armand-JV/SEN381-Project/issues/77)
- **Status:** Written, partially verified (no Docker on this machine)

### What was built
- `Dockerfile` (3 stages): Node 22 builds the Vite SPA into `wwwroot`; SDK publishes
  the backend with that `wwwroot`; ASP.NET runtime serves it (`Staging`, port 8080).
  `GIT_SHA` flows via environment at run time (see #79), not build args.
- `docker-compose.staging.yml`: `db` (postgres:18, volume, `pg_isready` healthcheck)
  + `web` (healthy-db dependency); secrets only via `.env.staging` (never committed,
  `${VAR:?err}` guards); `DemoAuth__Enabled=false` in staging.
- `.dockerignore`: excludes build outputs, git, secrets, and `wwwroot/*` (populated by the frontend stage).

### Verified without Docker
- `dotnet build slnx` clean (mirrors the backend stage); `npm ci` + `npm run build`
  green locally, and the produced `wwwroot` served live: `/` returns the SPA
  (`200`, title CivicConnect) — the #21 yield works end to end with a real build.
- NOT verified: `docker compose up` itself. Needs a Docker host (teammate/assessor/CI)
  before this is done-done; flagged honestly.

---

## M3-STG-04 (Issue #78) - Migration and seed procedure

- **Date:** 2026-10-15
- **Issue:** [#78](https://github.com/Armand-JV/SEN381-Project/issues/78)
- **Decided by:** P3 (user decision: EF bundle over SQL script)
- **Status:** Procedure recorded; bundle build verified, run pending scratch/staging DB

### Procedure (user-approved vehicle: EF bundle)
1. Build the bundle (verified — builds clean, no DB needed):
   `dotnet ef migrations bundle --project src/CivicConnect.Web --output efbundle(.exe)`
   (copy `appsettings.json` alongside it per the tooling note).
2. Start the staging database (compose `db` service).
3. Run `./efbundle` with the staging connection environment — applies pending migrations.
4. Seed: rely on `HasData` categories; demo users via TestAuth require
   `DemoAuth__Enabled=true` (staging default stays off; opt in per run as in #80).
5. Verify: `/health` green, `__EFMigrationsHistory` contains the expected IDs.

---

## M3-STG-07 (Issue #81) - Health check and failure evidence
- **Date:** 2026-10-15
- **Issue:** [#81](https://github.com/Armand-JV/SEN381-Project/issues/81)
- **Status:** Verified, no code change needed (`/health` with DB check already on `main`)

### Evidence (2026-10-15, Staging env, `DB_IP` overridden to unroutable `192.0.2.1`)
- Healthy baseline (earlier run): `200 {"status":"Healthy","database":"Connected"}`.
- DB-down run: `503 {"title":"Service Unavailable","status":503,"detail":"Cannot connect to database."}`.
- No connection strings, hosts, or credentials leak in either response (generic messages by design in `Program.cs`).
- Unexercised path: the `catch` branch (`"Database connection error."`) triggers only when the check itself throws rather than returning false; same no-detail property holds by construction.

---

## M3-TEST-15 (Issue #59) - Staging E2E run

- **Date:** 2026-10-15
- **Issue:** [#59](https://github.com/Armand-JV/SEN381-Project/issues/59)
- **Status:** Passing via `scripts/staging-e2e.ps1`; result tied to `/api/version` commit

### How it runs (all verified live 2026-10-15)
- Staging stack: app with `ASPNETCORE_ENVIRONMENT=Staging` (honest env — `--no-launch-profile`,
  after catching that the http profile forces Development) on the isolated scratch DB.
- Auth: TestAuth mints only in Development (controller design), so a throwaway Development
  minter on :5288 mints REQUESTER/STAFF/MANAGER tokens (same JWT config) spent on staging.
- Journey: submit → assign → InProgress → Resolved → requester reads Resolved → audit shows 2 entries.
- Latest run: `STAGING-E2E PASS: TC-S-01 green on commit ec9cb7a` (request #1010, 2 audit entries).
- Same minter pattern retrofitted to `scripts/staging-smoke.ps1` (it had the same latent flaw).
