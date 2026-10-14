# Database update request — P3 Phase 2 (for Armand)

> Copy/paste or send this file as-is. It asks Armand to apply one EF Core migration
> to the shared hosted database (`armanddatabase.webhop.me`), which only he controls.

---

Hi Armand,

Could you please apply the P3 Phase 2 migration to the shared database when you
get a moment? Everything is built, tested and committed on my side — I just can't
apply it myself since the database is yours.

## What it is

- **Migration:** `20261014131923_M3P3_AuditLog_And_QueryIndexes`
- **Branch:** `feature/m3-p3-phase2` (3 commits on top of `origin/main`: #21 API-only fix, #32 dashboard, #35 audit + #36 indexes)
- **Covers issues:** #35 (NFR-020 audit trail) + #36 (dashboard/queue indexes)

## What it changes (purely additive — no drops, no alters, no data loss)

1. New table `AuditLogs` (`Id`, `Actor`, `Action`, `EntityType`, `EntityId`, `BeforeSummary`, `AfterSummary`, `Timestamp`, `CorrelationId`).
2. Six indexes on `Requests`: `(State)`, `(RequesterId)`, `(AssigneeId)`, `(CreatedAt)`, `(State, CategoryId)`, `(AssigneeId, State)`.
3. Two indexes on `AuditLogs`: `(EntityType, EntityId)`, `(Timestamp)`.

Verified from the generated SQL: 1× `CREATE TABLE` + 8× `CREATE INDEX`, nothing else.
The idempotent full-schema script is in the repo at `src/CivicConnect.Web/db_schema.sql`.

## How to apply (either option)

**Option A — EF (recommended), from the repo root on `feature/m3-p3-phase2` with the `.env` present:**
```powershell
dotnet ef database update --project src/CivicConnect.Web/CivicConnect.Web.csproj
```

**Option B — psql**, run `src/CivicConnect.Web/db_schema.sql` (idempotent) against the database.

## How to verify it worked

```sql
\d "AuditLogs"
SELECT indexname FROM pg_indexes WHERE tablename IN ('Requests', 'AuditLogs') ORDER BY 1;
SELECT "MigrationId" FROM "__EFMigrationsHistory" ORDER BY 1;
```
Expect the `AuditLogs` table, the 8 indexes above, and a
`20261014131923_M3P3_AuditLog_And_QueryIndexes` row in the history table.

Smoke test (needs an Admin JWT from `GET /api/testauth/token/{user}?role=Admin`):
- `PUT /api/requests/{id}/state` → 200, then `GET /api/audit?entityType=Request&entityId={id}` → 200 with exactly one new `StateChanged` entry.
- `GET /api/reports/requests-summary` → 200 with reconciling counts.

## Rollback (if anything looks wrong)

```powershell
dotnet ef database update 20260930160638_InitialCreate --project src/CivicConnect.Web/CivicConnect.Web.csproj
```
This drops `AuditLogs` and the 8 indexes, restoring the previous schema. No other data is touched.

## Status on my side (already done, no action needed from you)

- Build green, `has-pending-model-changes` clean.
- Tests: 33/34 pass. The 1 failure is the pre-existing broken `TestWebApplicationFactory`
  (fails on `main` too — P2's harness, unrelated to this migration). My 5 new tests
  (3 dashboard TC-U-06 + 2 audit) all pass.
- Still deferred (not part of this request): assignment-audit wiring (blocked on P2's
  FR-008 assign endpoint), dashboard age buckets, and perf before/after evidence (M3-PERF phase).

Thanks!
— Riegardt (P3)
