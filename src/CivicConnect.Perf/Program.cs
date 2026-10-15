using System.Net.Http.Headers;
using System.Net.Http.Json;
using NBomber.CSharp;

// M3-PERF-02 (Issue #62): dashboard-filtering load scenario (TC-P-01).
// Plan (#61, P3-Decisions.md): 10 virtual users, 2 minutes steady, 1,000 seeded rows,
// p95 within 3s, zero errors, against a local/disposable staging database.
// Usage: dotnet run --project src/CivicConnect.Perf -- [baseUrl]
// Run order: dotnet ef database update, seed-perf.sql, then this scenario.

var baseUrl = args.Length > 0 ? args[0].TrimEnd('/') : "http://localhost:5287";

using var httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };

// Authenticate once: TestAuth mints MANAGER tokens in Development with DemoAuth enabled.
var tokenResponse = await httpClient.GetAsync("/api/testauth/token/perf-loader?role=MANAGER");
tokenResponse.EnsureSuccessStatusCode();
var token = (await tokenResponse.Content.ReadFromJsonAsync<TokenPayload>())?.Token
    ?? throw new InvalidOperationException("TestAuth did not return a token.");
httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

var step = 0;

var scenario = Scenario.Create("dashboard_summary", async _ =>
{
    // Alternate unfiltered and filtered dashboard reads, as a manager would.
    var path = Interlocked.Increment(ref step) % 2 == 0
        ? "/api/reports/requests-summary"
        : "/api/reports/requests-summary?state=Open&categoryId=1";
    var response = await httpClient.GetAsync(path);
    return response.IsSuccessStatusCode ? Response.Ok() : Response.Fail();
})
.WithLoadSimulations(Simulation.KeepConstant(copies: 10, during: TimeSpan.FromMinutes(2)));

NBomberRunner
    .RegisterScenarios(scenario)
    .WithReportFolder("perf-reports")
    .Run();

internal sealed class TokenPayload
{
    public string Token { get; set; } = string.Empty;
}
