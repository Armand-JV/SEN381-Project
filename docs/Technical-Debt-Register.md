# Technical Debt Register (M3)

Owner of the register: P3 (M3-REL-04, issue #101). Columns per the issue:
item | impact | interest (cost of delay) | plan | owner | status.
Seed verified against the tree on 2026-10-14 (branch `feature/m3-p3-phase3`,
i.e. current `origin/main`). Keep updated as debt is accepted or retired.

| ID | Item (evidence) | Impact | Interest | Plan | Owner | Status |
|---|---|---|---|---|---|---|
| TD-01 | Repository abstraction bypassed: `RequestsController` uses `CivicConnectDbContext` directly; `IRequestRepository`/`ICategoryRepository` unimplemented | Testability + layering erosion (mocking needs a database) | Grows with every new endpoint written the same way | ADR M3-CONF-02 decision (implement repos vs document DbContext-as-UoW) | P1 | Open, pending ADR |
| TD-02 | No optimistic concurrency token on `Request` (`Request.cs`: no `RowVersion`/`ConcurrencyCheck`) | Silent last-writer-wins on concurrent staff edits | Grows with concurrent staff usage | Add token + 409-stale handling | P2 | Open |
| TD-03 | Observer notification not outbox-based: `LogRequestStateObserver` is log-only best-effort (`#42` verified: failure isolated, but delivery not guaranteed) | Lost notifications on process failure; no retry | Grows if notification becomes relied upon | Outbox deferred beyond M3 (accepted) | P3 | Accepted |
| TD-04 | Two audit tables side by side: `AuditEntries` (FK-cascade, request-only) vs `AuditLogs` (spec-shaped, standalone) | Split evidence, reviewer confusion | Blocks assignment-audit wiring + release cleanliness | One-table decision (P3 proposal: keep `AuditLogs`, deprecate `AuditEntries`) | P3 + Armand | Open, awaiting team call |
| TD-05 | Dead authorization path: `AccessControlService.CanAccess` (`IsInRole("Admin")`) injected but never called; live checks use uppercase role policies | Divergent role strings invite the wrong check in new code | Next endpoint may copy the dead pattern | Delete service or reconcile to one convention | P2 | Open |
| TD-06 | `TestAuthController` crashes when `JWT_SECRET` is missing (`Encoding.UTF8.GetBytes(secret)`, no fallback unlike `Program.cs`) | Broken local demo/test-token flow on clean machines | Hits every new contributor without `.env` | Validate + clear error (or shared fallback) | P2 | Open |
| TD-07 | Audit read capped at `Take(200)` with no paging (`AuditController`, P3's own code) | Unusable log browsing as volume grows | Grows with every audited action | Add `page`/`pageSize` like the staff queue | P3 | Accepted, planned |
| TD-08 | Swagger only in Development (`Program.cs`); staging demo path undecided | Assessors cannot explore the API on staging builds | Blocks M3-STG demo story | Feature-flag Swagger for staging (ties M3-STG-01 decision) | P3 | Open |
| TD-09 | Dead migration file `20261001000000_AddAuditEntries` (duplicates `...13145559`, invisible to EF) | Next `database update` confusion / false alarm | Static until someone investigates again | Delete the file | Armand | Open |
| TD-10 | Deferred scope still undefined: FR-015/016 have no acceptance criteria; dashboard age buckets, `Created`-audit question open | Cannot plan or estimate that work | Grows if scope creeps in late | Scope-freeze decisions before Phase 5 | All (P3 drives 015/016 wording via RTM) | Open |
| TD-11 | `UseExceptionHandler("/Home/Error")` points at a non-existent controller on `main` | Real 500s render as framework pages, no ProblemDetails | Until Phase 2 PR merges | Covered by Phase 2 PR (`AddProblemDetails` + handler fix) | P3 | Pending merge |

Retired this session: broken `TestWebApplicationFactory` (fixed by #123 — suite 59/59);
duplicate JWT registration crash (fixed in Phase 2 merge); `UpdateState` open transitions
(fixed by `RequestStateTransitionPolicy`, #121).
