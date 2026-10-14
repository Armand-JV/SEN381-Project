# P3 Decisions (M3 - Release Candidate)

Owner: Person 3 (Release, Performance & Operations).
Purpose: record every P3 decision with rationale, alternatives, and M2 comparison. Each issue gets a new `##` heading.

---

## M3-UI-01 (Issue #21) — API-only demo path, no Razor UI

- **Date:** 2026-10-14
- **Issue:** [#21 — Decide and build minimal UI (or documented API-only demo path)](https://github.com/Armand-JV/SEN381-Project/issues/21)
- **Decided by:** P3 (user decision, asked explicitly per workflow)
- **Status:** Implemented on `feature/m3-ui-01-decide-and-build-minimal-ui` (pending build verify, commit, PR)

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
