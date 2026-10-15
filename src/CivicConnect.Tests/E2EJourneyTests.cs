using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using CivicConnect.Domain.Enums;
using CivicConnect.Web.Controllers;
using CivicConnect.Web.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CivicConnect.Tests;

// TC-S-01 (M3-TEST-13, Issue #57): submit -> staff assigns and resolves -> requester sees update.
// API-driven per M3-UI-01. History is asserted at the database level: FR-005's
// GET /api/requests/{id}/history endpoint does not exist on main (P1 scope gap, flagged).
public class E2EJourneyTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public E2EJourneyTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _factory.ResetDatabase();
    }

    [Fact]
    public async Task SubmitAssignResolve_RequesterSeesUpdate()
    {
        var requester = _factory.CreateClient().AsRequester("req-1");
        var staff = _factory.CreateClient().AsStaff("staff-1");

        // 1. Requester submits.
        var createResponse = await requester.PostAsJsonAsync(
            "/api/requests",
            new CreateRequestDto { Title = "E2E pothole", Description = "E2E description", CategoryId = 1 });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<CreatedRequest>();
        Assert.NotNull(created);

        // 2. Staff assigns.
        var assignResponse = await staff.PutAsJsonAsync(
            $"/api/requests/{created!.Id}/assign",
            new AssignRequestDto { AssigneeId = "staff-1", Reason = "E2E triage" });
        Assert.Equal(HttpStatusCode.OK, assignResponse.StatusCode);

        // 3. Staff progresses to InProgress, then Resolved.
        foreach (var next in new[] { RequestState.InProgress, RequestState.Resolved })
        {
            var stateResponse = await staff.PutAsJsonAsync(
                $"/api/requests/{created.Id}/state",
                new UpdateStateDto { NewState = next, Reason = "E2E step" });
            Assert.Equal(HttpStatusCode.OK, stateResponse.StatusCode);
        }

        // 4. Requester sees the resolved request.
        var getResponse = await requester.GetAsync($"/api/requests/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        // 5. History matches the transitions performed (asserted at DB level;
        // FR-005 history endpoint is not implemented on main).
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CivicConnectDbContext>();
        var states = await db.StatusHistories
            .Where(h => h.RequestId == created.Id)
            .OrderBy(h => h.Id)
            .Select(h => h.NewState)
            .ToListAsync();
        Assert.Equal(
            new[] { RequestState.Open, RequestState.Assigned, RequestState.InProgress, RequestState.Resolved },
            states);
    }

    private sealed class CreatedRequest
    {
        public int Id { get; set; }
    }
}
