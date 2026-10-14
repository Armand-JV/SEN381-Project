using System.Net;
using System.Net.Http.Json;
using CivicConnect.Domain.Entities;
using CivicConnect.Domain.Enums;
using CivicConnect.Web.Controllers;
using CivicConnect.Web.Data;
using Xunit;

namespace CivicConnect.Tests;

public class RequestControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public RequestControllerTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _factory.ResetDatabase();
    }

    [Fact]
    public async Task UpdateState_ClosedToOpen_ReturnsBadRequest()
    {
        var client = _factory.CreateClient().AsStaff("staff1");
        var request = await SeedRequest(RequestState.Closed, requesterId: "user1", assigneeId: "staff1");

        var response = await client.PutAsJsonAsync($"/api/requests/{request.Id}/state",
            new UpdateStateDto { NewState = RequestState.Open, Reason = "Should fail" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData(RequestState.Rejected)]
    [InlineData(RequestState.Cancelled)]
    [InlineData(RequestState.Closed)]
    public async Task UpdateState_TerminalStateWithReason_PersistsHistory(RequestState terminalState)
    {
        var client = _factory.CreateClient().AsStaff("staff1");
        var sourceState = terminalState == RequestState.Closed ? RequestState.Resolved : RequestState.Open;
        var request = await SeedRequest(sourceState, requesterId: "user1", assigneeId: "staff1");

        var response = await client.PutAsJsonAsync($"/api/requests/{request.Id}/state",
            new UpdateStateDto { NewState = terminalState, Reason = "Completed after review" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var db = _factory.CreateDbContext();
        var history = db.StatusHistories.Single(h => h.RequestId == request.Id);
        Assert.Equal(terminalState, history.NewState);
        Assert.Equal("Completed after review", history.Reason);
    }

    [Theory]
    [InlineData(RequestState.Rejected)]
    [InlineData(RequestState.Cancelled)]
    [InlineData(RequestState.Closed)]
    public async Task UpdateState_TerminalStateWithoutReason_ReturnsBadRequest(RequestState terminalState)
    {
        var client = _factory.CreateClient().AsStaff("staff1");
        var sourceState = terminalState == RequestState.Closed ? RequestState.Resolved : RequestState.Open;
        var request = await SeedRequest(sourceState, requesterId: "user1", assigneeId: "staff1");

        var response = await client.PutAsJsonAsync($"/api/requests/{request.Id}/state",
            new UpdateStateDto { NewState = terminalState, Reason = "  " });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData(RequestState.Closed)]
    [InlineData(RequestState.Cancelled)]
    [InlineData(RequestState.Rejected)]
    public async Task UpdateState_FromAnyTerminalState_ReturnsBadRequest(RequestState sourceState)
    {
        var client = _factory.CreateClient().AsStaff("staff1");
        var request = await SeedRequest(sourceState, requesterId: "user1", assigneeId: "staff1");

        var response = await client.PutAsJsonAsync($"/api/requests/{request.Id}/state",
            new UpdateStateDto { NewState = RequestState.Open, Reason = "Should fail" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetRequest_StaffOutsideAssignment_ReturnsForbidden()
    {
        var client = _factory.CreateClient().AsStaff("staff2");
        var request = await SeedRequest(RequestState.Assigned, requesterId: "user1", assigneeId: "staff1");

        var response = await client.GetAsync($"/api/requests/{request.Id}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private async Task<Request> SeedRequest(RequestState state, string requesterId, string? assigneeId)
    {
        using var db = _factory.CreateDbContext();
        var category = new Category { Name = $"Category-{Guid.NewGuid()}" };
        db.Categories.Add(category);
        await db.SaveChangesAsync();

        var request = new Request
        {
            CategoryId = category.Id,
            Title = "Title",
            Description = "Desc",
            State = state,
            RequesterId = requesterId,
            AssigneeId = assigneeId
        };
        db.Requests.Add(request);
        await db.SaveChangesAsync();
        return request;
    }
}
