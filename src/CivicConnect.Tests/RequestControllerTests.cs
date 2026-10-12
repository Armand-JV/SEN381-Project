using System.Net;
using System.Net.Http.Json;
using System.Linq;
using System.Threading.Tasks;
using CivicConnect.Domain.Enums;
using CivicConnect.Web.Controllers;
using Xunit;
using CivicConnect.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using CivicConnect.Web.Data;

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
        // Arrange
        var client = _factory.CreateClient().AsStaff();
        var db = _factory.CreateDbContext();

        var category = new Category { Name = "TestCategory" };
        db.Categories.Add(category);
        await db.SaveChangesAsync();

        var request = new Request { CategoryId = category.Id, Title = "Title", Description = "Desc", State = RequestState.Closed };
        db.Requests.Add(request);
        await db.SaveChangesAsync();

        var updateDto = new UpdateStateDto { NewState = RequestState.Open, Reason = "Should fail" };

        // Act
        var response = await client.PutAsJsonAsync($"/api/requests/{request.Id}/state", updateDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetStaffRequests_Requester_ReturnsForbidden()
    {
        var client = _factory.CreateClient().AsRequester("requester-1");

        var response = await client.GetAsync("/api/staff/requests");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetStaffRequests_CombinesFiltersAndPaging()
    {
        var db = _factory.CreateDbContext();
        db.Requests.AddRange(
            new Request
            {
                CategoryId = 1,
                Title = "Matching request",
                Description = "Description",
                RequesterId = "requester-1",
                AssigneeId = "staff-a",
                State = RequestState.Assigned,
                CreatedAt = DateTime.UtcNow.AddMinutes(-1)
            },
            new Request
            {
                CategoryId = 1,
                Title = "Different assignee",
                Description = "Description",
                RequesterId = "requester-2",
                AssigneeId = "staff-b",
                State = RequestState.Assigned,
                CreatedAt = DateTime.UtcNow.AddMinutes(-2)
            },
            new Request
            {
                CategoryId = 2,
                Title = "Different category",
                Description = "Description",
                RequesterId = "requester-3",
                AssigneeId = "staff-a",
                State = RequestState.Open,
                CreatedAt = DateTime.UtcNow.AddMinutes(-3)
            });
        await db.SaveChangesAsync();

        var response = await _factory.CreateClient().AsStaff("staff-1")
            .GetFromJsonAsync<PagedResultDto<StaffRequestListItemDto>>(
                "/api/staff/requests?state=Assigned&categoryId=1&assigneeId=staff-a&page=1&pageSize=10");

        Assert.NotNull(response);
        Assert.Equal(1, response.TotalCount);
        var item = Assert.Single(response.Items);
        Assert.Equal("Matching request", item.Title);
    }

    [Fact]
    public async Task GetRequest_SeededRequestIncludesComment()
    {
        var request = await _factory.CreateClient()
            .GetFromJsonAsync<Request>("/api/requests/2");

        Assert.NotNull(request);
        var comment = Assert.Single(request.Comments);
        Assert.Equal("Thank you for reporting this issue. Our team will investigate it.", comment.Text);
        Assert.False(comment.IsInternal);
    }

    [Fact]
    public async Task AssignRequest_UpdatesStateHistoryAndAuditWithActor()
    {
        var db = _factory.CreateDbContext();
        var request = new Request
        {
            CategoryId = 1,
            Title = "Needs assignment",
            Description = "Description",
            RequesterId = "requester-1",
            State = RequestState.Open,
            CreatedAt = DateTime.UtcNow
        };
        db.Requests.Add(request);
        await db.SaveChangesAsync();

        var client = _factory.CreateClient().AsStaff("staff-7");
        var response = await client.PutAsJsonAsync(
            $"/api/requests/{request.Id}/assign",
            new AssignRequestDto { AssigneeId = "staff-9", Reason = "Capacity match" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var verificationDb = _factory.CreateDbContext();
        var persisted = verificationDb.Requests.Single(item => item.Id == request.Id);
        Assert.Equal(RequestState.Assigned, persisted.State);
        Assert.Equal("staff-9", persisted.AssigneeId);
        var history = verificationDb.StatusHistories.Single(item => item.RequestId == request.Id);
        Assert.Equal("staff-7", history.ChangedBy);
        var audit = verificationDb.AuditEntries.Single(item => item.RequestId == request.Id);
        Assert.Equal("RequestAssigned", audit.Action);
        Assert.Equal("staff-7", audit.ActorId);
    }

    [Fact]
    public async Task UpdateState_IllegalTransition_LeavesRequestUnchanged()
    {
        var db = _factory.CreateDbContext();
        var request = new Request
        {
            CategoryId = 1,
            Title = "Open request",
            Description = "Description",
            RequesterId = "requester-1",
            State = RequestState.Open,
            CreatedAt = DateTime.UtcNow
        };
        db.Requests.Add(request);
        await db.SaveChangesAsync();

        var response = await _factory.CreateClient().AsStaff("staff-1").PutAsJsonAsync(
            $"/api/requests/{request.Id}/state",
            new UpdateStateDto { NewState = RequestState.Resolved, Reason = "Invalid jump" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(RequestState.Open, db.Requests.Single(item => item.Id == request.Id).State);
        Assert.Empty(db.StatusHistories.Where(item => item.RequestId == request.Id));
    }
}
