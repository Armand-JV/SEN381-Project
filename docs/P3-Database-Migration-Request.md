# Database update record — P3 Phase 2 (for Armand / team record)

> Status 2026-10-14: **APPLIED by Riegardt (P3) with Armand's go-ahead.**
> This file is now a record of what was done, not a request.

---

Hi Armand,

Applied the P3 Phase 2 migration to the shared database (`armanddatabase.webhop.me`)
as agreed. Details below for your records.

## What it was

- **Migration:** `20261014145832_M3P3AuditLogAndQueryIndexes` (class renamed for the
  CA1707 analyzer rule; same timestamp/file as before)
- **Branch:** `feature/m3-p3-phase2` (#21 API-only fix, #32 dashboard, #35 audit + #36 indexes)
- **Covers issues:** #35 (NFR-020 audit trail) + #36 (dashboard/queue indexes)

## What it changed (purely additive — no drops, no alters, no data loss)

1. New table `AuditLogs` (`Id`, `Actor`, `Action`, `EntityType`, `EntityId`, `BeforeSummary`, `AfterSummary`, `Timestamp`, `CorrelationId`).
2. Six indexes on `Requests`: `(State)`, `(RequesterId)`, `(AssigneeId)`, `(CreatedAt)`, `(State, CategoryId)`, `(AssigneeId, State)`.
3. Two indexes on `AuditLogs`: `(EntityType, EntityId)`, `(Timestamp)`.

## What else was done to get it applied (metadata only, zero data changed)

- `20261012153000_SeedRequestTwo` was marked as applied in `__EFMigrationsHistory`
  **without running it**: its PKs (`Requests.Id=2`, `Comments.Id=1`) are occupied by
  live rows with different content (`Broken Sidewalk`/`UserA`, not the seed values),
  so running it would have failed *and* threatened live data. Live data wins; recorded only.
- The stray `20261001000000_AddAuditEntries` file turned out to be invisible to EF
  (not discovered, not in the pending list), so no action was needed for the apply —
  **but please delete that dead file**, it will confuse the next person.
- `__EFMigrationsHistory` now: `InitialCreate`, `SeedRequestTwo` (baselined),
  `AddAuditEntriesTable`, `M3P3AuditLogAndQueryIndexes` (applied).

## Verification (all run 2026-10-14)

- `AuditLogs` table + all 8 indexes present, confirmed via `pg_indexes`.
- Live end-to-end on the shared DB: MANAGER assign (200) → state to InProgress (200)
  → `GET /api/audit?entityType=Request&entityId=4` returns exactly the new
  `StateChanged Assigned→InProgress` row → dashboard reconciles (`5+1+1=7`).
- Suite: 59/59 pass (their 3 authz-mismatched tests were fixed on main's side by #123).

## Rollback (if anything looks wrong)

```powershell
dotnet ef database update 20261013145559_AddAuditEntriesTable --project src/CivicConnect.Web/CivicConnect.Web.csproj
```
This removes only our migration (`AuditLogs` + 8 indexes). The baselined seed row can be
deleted from history if you want it fully pristine. No other data is touched.

## Still open (not part of this apply)

- One-table audit question (`AuditEntries` vs `AuditLogs`) — both tables are now
  collecting rows side by side; needs your call (see P3-Decisions.md).
- Assignment-audit repoint, dashboard age buckets, perf before/after evidence.

Thanks!
— Riegardt (P3)
