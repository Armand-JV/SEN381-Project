# PED v3 s8 + s10 — Performance/reliability and release readiness (P3, M3-PED-05 #93)

Author: P3. Status: draft sections for P1 to merge into PED v3.0. Evidence links
point at repo artefacts; nothing here duplicates them.

## s8 — Performance and reliability

**Workload (M3-PERF-01 #61, decided by P3):** dashboard filtering
(`GET /api/reports/requests-summary`, filtered + unfiltered), NBomber 6.7.0,
10 virtual users x 2 minutes, 1,000 deterministic seeded rows, local app +
isolated local PostgreSQL. Harness: `src/CivicConnect.Perf`; seed:
`src/CivicConnect.Perf/seed-perf.sql`; plan: `docs/P3-Decisions.md` M3-PERF-01.

**Result (M3-PERF-03 #63, TC-P-01, run 2026-10-15):** 109,525 requests, 0 errors,
~913 RPS; p50 9.45ms, p75 14.27ms, **p95 23.71ms**, p99 32.9ms, max 486ms.
Target (NFR-024 as clarified by CR-001): p95 within 3s — **PASS** by ~100x.

**Interpretation:** the dashboard query path (served by the M3-DATA-01 #36 indexes)
has large headroom under the defined workload. The shared database was untouched;
all load ran against the isolated scratch copy.

**Limits (what this does not prove):** production behavior (different host, volume,
network); concurrent-write contention (read-only workload); beyond-10-user scale;
cold-start effects. NBomber free-license messaging appeared at runtime — academic
use here; flag before any organizational use. See `docs/P3-Decisions.md` M3-PERF-01.

**Reliability:** `/health` reports database connectivity (200 Healthy, 503 with
generic message on DB-down, no leaked details — proven live, M3-STG-07 #81).
Rollback/recovery procedure with dry-run evidence: `docs/P3-Decisions.md` M3-STG-09.
Forward-fix is policy; `Down()` drops tables (proven: 5 audit rows lost on scratch),
so rollbacks require a prior `pg_dump` backup.

## s10 — Release readiness

**Evidence summary:** `docs/Release-Evidence-Summary.md` (M3-REL-01 #98) — nine areas
with results, interpretations, concerns, and links.

**Classification (M3-REL-02 #99, P3 judgment call): CONDITIONALLY READY.**
Reasoning: clean build under warnings-as-errors; 94/94 tests incl. E2E locally and
on staging; perf PASS; static/security gates clean; audit writes verified; full
evidence indexed. Against READY: no production host/TLS/IdP, local-simulated
staging, dual audit tables unresolved, self-authored FR-015/016 AC, best-effort
notifications, no concurrency token.

**Conditions:** demo/assessment use only via documented paths; no production traffic
until real IdP + hosting/TLS story + one-table audit decision; deferred scope stays
deferred.

**Residual risks:** `docs/Residual-Risk-Statement.md` RR-REL-01..06 (P3, M3-REL-03 #100);
security rows RR-SEC-01..04 (P2). **Debt:** `docs/Technical-Debt-Register.md` TD-01..TD-11.

**Staging and demo:** compose-local target with proven local interim (M3-STG-01 #75);
staging config, version endpoint, smoke and staging-E2E scripts green (M3-STG-02/05/06,
M3-TEST-15); environment differences and demo path in `docs/P3-Decisions.md`.
