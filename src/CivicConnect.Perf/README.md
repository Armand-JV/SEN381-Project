# CivicConnect.Perf — dashboard load scenario (M3-PERF-02, TC-P-01)

NBomber console harness for the #61 plan: 10 virtual users × 2 minutes against
`GET /api/reports/requests-summary` (alternating unfiltered/filtered), targeting
p95 ≤ 3s with zero errors (NFR-024).

## Run order (local/disposable staging DB only — never the shared remote DB)

```powershell
# 1. Migrate the staging database
dotnet ef database update --project src/CivicConnect.Web

# 2. Seed 1,000 deterministic rows (rerunnable)
psql -h <staging-host> -U <user> -d <db> -f src/CivicConnect.Perf/seed-perf.sql

# 3. Run the app (Development, DemoAuth on for TestAuth tokens)
dotnet run --project src/CivicConnect.Web --launch-profile http

# 4. Run the load scenario (separate shell)
dotnet run --project src/CivicConnect.Perf -- http://localhost:5287
```

Reports land in `perf-reports/`. Interpretation (pass/fail vs target, limitations)
happens in M3-PERF-03 (#63).
