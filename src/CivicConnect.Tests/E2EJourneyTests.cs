using System.Collections.Generic;
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
// API-driven per M3-UI-01, including requester-visible state and history assertions.
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

        // 4. Requester sees the resolved state in the detail response.
        var detail = await requester.GetFromJsonAsync<RequestDetailDto>($"/api/requests/{created.Id}");
        Assert.NotNull(detail);
        Assert.Equal(RequestState.Resolved, detail.State);

        // 5. Requester reads the committed status history through its API endpoint.
        var history = await requester.GetFromJsonAsync<List<StatusHistoryDto>>(
            $"/api/requests/{created.Id}/history");
        Assert.NotNull(history);
        Assert.Equal(
            new[] { RequestState.Open, RequestState.Assigned, RequestState.InProgress, RequestState.Resolved },
            history.Select(item => item.NewState));
        Assert.Equal(RequestState.Resolved, detail.History[^1].NewState);
    }

    private sealed class CreatedRequest
    {
        public int Id { get; set; }
    }

    // TC-S-02 (M3-TEST-14, Issue #58): resolved request -> feedback -> dashboard reflects it.
    [Fact]
    public async Task ResolvedFeedback_DashboardReflectsIt()
    {
        var requester = _factory.CreateClient().AsRequester("req-2");
        var staff = _factory.CreateClient().AsStaff("staff-2");
        var manager = _factory.CreateClient().AsManager();

        // 1-3. Submit, assign, resolve (same proven path as TC-S-01).
        var createResponse = await requester.PostAsJsonAsync(
            "/api/requests",
            new CreateRequestDto { Title = "E2E feedback", Description = "TC-S-02", CategoryId = 2 });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<CreatedRequest>();
        Assert.NotNull(created);

        var assignResponse = await staff.PutAsJsonAsync(
            $"/api/requests/{created!.Id}/assign",
            new AssignRequestDto { AssigneeId = "staff-2", Reason = "E2E triage" });
        Assert.Equal(HttpStatusCode.OK, assignResponse.StatusCode);

        foreach (var next in new[] { RequestState.InProgress, RequestState.Resolved })
        {
            var stateResponse = await staff.PutAsJsonAsync(
                $"/api/requests/{created.Id}/state",
                new UpdateStateDto { NewState = next, Reason = "E2E step" });
            Assert.Equal(HttpStatusCode.OK, stateResponse.StatusCode);
        }

        // 4. Requester submits feedback (owner only, resolved only).
        var feedbackResponse = await requester.PostAsJsonAsync(
            $"/api/requests/{created.Id}/feedback",
            new { Rating = 5, Comment = "Fixed quickly, thank you." });
        Assert.Equal(HttpStatusCode.Created, feedbackResponse.StatusCode);

        // 5. Owner reads it back.
        var feedbackGet = await requester.GetAsync($"/api/requests/{created.Id}/feedback");
        Assert.Equal(HttpStatusCode.OK, feedbackGet.StatusCode);

        // 6. Manager dashboard reflects the resolved request.
        var summary = await manager.GetFromJsonAsync<DashboardSummaryDto>(
            "/api/reports/requests-summary?categoryId=2");
        Assert.NotNull(summary);
        Assert.Equal(1, summary!.Total);
        Assert.Equal(1, summary.ByState["Resolved"]);
        Assert.Equal(1, summary.ByCategory["2"]);
    }

    private sealed class DashboardSummaryDto
    {
        public int Total { get; set; }
        public Dictionary<string, int> ByState { get; set; } = new();
        public Dictionary<string, int> ByCategory { get; set; } = new();
    }
}
