-- M3-PERF-02 seed: 1,000 deterministic dashboard-load rows (TC-P-01 data set).
-- Run order: dotnet ef database update, then this script, then the NBomber scenario.
-- Rerunnable: removes previous perf-seed rows (and their dependent history/audit rows) first.
-- Target: a local/disposable PostgreSQL staging database. NEVER the shared remote DB.

BEGIN;

DELETE FROM "StatusHistories"
WHERE "RequestId" IN (SELECT "Id" FROM "Requests" WHERE "RequesterId" LIKE 'perf-seed-%');

DELETE FROM "AuditEntries"
WHERE "RequestId" IN (SELECT "Id" FROM "Requests" WHERE "RequesterId" LIKE 'perf-seed-%');

DELETE FROM "Requests" WHERE "RequesterId" LIKE 'perf-seed-%';

INSERT INTO "Requests"
    ("Title", "Description", "CategoryId", "State", "CreatedAt", "RequesterId", "AssigneeId")
SELECT
    'Perf seed request ' || g,
    'Deterministic load-test row ' || g,
    (g % 3) + 1,
    (g % 5),
    NOW() - ((g % 90) || ' days')::INTERVAL,
    'perf-seed-' || LPAD((g % 50)::TEXT, 3, '0'),
    CASE WHEN (g % 5) IN (1, 2, 3) THEN 'perf-staff-' || LPAD((g % 10)::TEXT, 2, '0') ELSE NULL END
FROM generate_series(1, 1000) AS g;

COMMIT;
