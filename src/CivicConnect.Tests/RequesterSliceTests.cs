using System.Net;
using System.Net.Http.Json;
using CivicConnect.Domain.Entities;
using CivicConnect.Domain.Enums;
using CivicConnect.Web.Controllers;
using CivicConnect.Web.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CivicConnect.Tests;

public sealed class RequesterSliceTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public RequesterSliceTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _factory.ResetDatabase();
    }

    [Fact]
    public async Task Categories_ReturnSeededDtos()
    {
        var response = await _factory.CreateClient().GetFromJsonAsync<List<CategoryDto>>("/api/categories");
        Assert.NotNull(response);
        Assert.Contains(response, category => category.Id == 1 && category.Name == "Infrastructure");
    }

    [Theory]
    [InlineData(" ", "Description", 1)]
    [InlineData("x", " ", 1)]
    [InlineData("x", "Description", 0)]
    [InlineData("", "", 0)]
    public async Task CreateRequest_InvalidInput_ReturnsFieldValidationAndPersistsNothing(string title, string description, int categoryId)
    {
        var response = await _factory.CreateClient().AsRequester("requester-validation")
            .PostAsJsonAsync("/api/requests", new CreateRequestDto { Title = title, Description = description, CategoryId = categoryId });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        using var db = _factory.CreateDbContext();
        Assert.DoesNotContain(db.Requests, request => request.RequesterId == "requester-validation");
    }

    [Fact]
    public async Task CreateRequest_MaxLengthsAccepted_AndOverLimitRejected()
    {
        var client = _factory.CreateClient().AsRequester("requester-boundary");
        var accepted = await client.PostAsJsonAsync("/api/requests", new CreateRequestDto
        {
            Title = new string('t', 200),
            Description = new string('d', 2000),
            CategoryId = 1
        });
        Assert.Equal(HttpStatusCode.Created, accepted.StatusCode);

        var rejected = await client.PostAsJsonAsync("/api/requests", new CreateRequestDto
        {
            Title = new string('t', 201),
            Description = "Description",
            CategoryId = 1
        });
        Assert.Equal(HttpStatusCode.BadRequest, rejected.StatusCode);
    }

    [Fact]
    public async Task GetMyRequests_UsesActorAndReturnsPagedStateAndCategory()
    {
        using (var db = _factory.CreateDbContext())
        {
            db.Requests.AddRange(
                MakeRequest("requester-page", "first", createdAt: new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)),
                MakeRequest("requester-page", "second", createdAt: new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc)),
                MakeRequest("other-requester", "private"));
            await db.SaveChangesAsync();
        }

        var response = await _factory.CreateClient().AsRequester("requester-page")
            .GetFromJsonAsync<PagedResultDto<RequesterRequestListItemDto>>("/api/requests/mine?page=1&pageSize=1");
        Assert.NotNull(response);
        Assert.Equal(2, response.TotalCount);
        Assert.Equal("second", Assert.Single(response.Items).Title);
        Assert.Equal("Infrastructure", response.Items[0].CategoryName);
        Assert.Equal("Open", response.Items[0].StateName);

        var staffResponse = await _factory.CreateClient().AsStaff("staff-page").GetAsync("/api/requests/mine");
        Assert.Equal(HttpStatusCode.Forbidden, staffResponse.StatusCode);
        var anonymousResponse = await _factory.CreateClient().GetAsync("/api/requests/mine");
        Assert.Equal(HttpStatusCode.Unauthorized, anonymousResponse.StatusCode);
    }

    [Fact]
    public async Task GetMyRequests_InvalidPaging_ReturnsBadRequest()
    {
        var response = await _factory.CreateClient().AsRequester("requester-page")
            .GetAsync("/api/requests/mine?page=0&pageSize=101");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetHistory_IncludesInitialAndChronologicallyOrdersRows_AndEnforcesOwnership()
    {
        var request = MakeRequest("requester-history", "history");
        using (var db = _factory.CreateDbContext())
        {
            db.Requests.Add(request);
            await db.SaveChangesAsync();
            db.StatusHistories.AddRange(
                new StatusHistory { RequestId = request.Id, PreviousState = RequestState.Open, NewState = RequestState.Open, ChangedBy = "requester-history", ChangedAt = DateTime.UtcNow.AddMinutes(-2), Reason = "Initial submission" },
                new StatusHistory { RequestId = request.Id, PreviousState = RequestState.Open, NewState = RequestState.Assigned, ChangedBy = "staff", ChangedAt = DateTime.UtcNow.AddMinutes(-1), Reason = "Assigned" });
            await db.SaveChangesAsync();
        }

        var rows = await _factory.CreateClient().AsRequester("requester-history")
            .GetFromJsonAsync<List<StatusHistoryDto>>($"/api/requests/{request.Id}/history");
        Assert.NotNull(rows);
        Assert.Equal(2, rows.Count);
        Assert.Equal("Initial submission", rows[0].Reason);
        Assert.Equal(RequestState.Assigned, rows[1].NewState);
        Assert.Equal(HttpStatusCode.Forbidden, (await _factory.CreateClient().AsRequester("other")
            .GetAsync($"/api/requests/{request.Id}/history")).StatusCode);
    }

    [Fact]
    public async Task RequestDetail_ProjectsSafeDtoAndFiltersInternalComments()
    {
        using (var db = _factory.CreateDbContext())
        {
            db.Comments.AddRange(
                new Comment { RequestId = 2, Text = "Public", CreatedBy = "staff", CreatedAt = DateTime.UtcNow },
                new Comment { RequestId = 2, Text = "Secret", CreatedBy = "staff", CreatedAt = DateTime.UtcNow, IsInternal = true });
            await db.SaveChangesAsync();
        }

        var response = await _factory.CreateClient().AsRequester("requester-2").GetAsync("/api/requests/2");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        Assert.Contains("categoryName", json, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Public", json);
        Assert.DoesNotContain("Secret", json);
    }

    [Theory]
    [InlineData(RequestState.Open, HttpStatusCode.Conflict)]
    [InlineData(RequestState.Resolved, HttpStatusCode.Created)]
    [InlineData(RequestState.Closed, HttpStatusCode.Created)]
    public async Task Feedback_RequiresCompletionAndValidOwner(RequestState state, HttpStatusCode expected)
    {
        var request = await SeedRequest("feedback-owner", state);
        var response = await _factory.CreateClient().AsRequester("feedback-owner")
            .PostAsJsonAsync($"/api/requests/{request.Id}/feedback", new SubmitFeedbackDto { Rating = 4, Comment = "Thanks" });
        Assert.Equal(expected, response.StatusCode);
        if (expected == HttpStatusCode.Created)
        {
            var feedback = await response.Content.ReadFromJsonAsync<FeedbackDto>();
            Assert.NotNull(feedback);
            Assert.Equal("feedback-owner", feedback.CreatedBy);
        }
    }

    [Fact]
    public async Task Feedback_RejectsNonOwnerInvalidRatingAndDuplicate()
    {
        var request = await SeedRequest("feedback-owner", RequestState.Resolved);
        var client = _factory.CreateClient();
        Assert.Equal(HttpStatusCode.Forbidden, (await client.AsRequester("intruder").PostAsJsonAsync(
            $"/api/requests/{request.Id}/feedback", new SubmitFeedbackDto { Rating = 4 })).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.AsRequester("feedback-owner").PostAsJsonAsync(
            $"/api/requests/{request.Id}/feedback", new SubmitFeedbackDto { Rating = 6 })).StatusCode);
        var valid = await client.AsRequester("feedback-owner").PostAsJsonAsync(
            $"/api/requests/{request.Id}/feedback", new SubmitFeedbackDto { Rating = 5, Comment = "  Good  " });
        Assert.Equal(HttpStatusCode.Created, valid.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await client.AsRequester("feedback-owner").PostAsJsonAsync(
            $"/api/requests/{request.Id}/feedback", new SubmitFeedbackDto { Rating = 3 })).StatusCode);
        using var db = _factory.CreateDbContext();
        var saved = Assert.Single(db.Feedback.Where(item => item.RequestId == request.Id));
        Assert.Equal("Good", saved.Comment);
        Assert.Equal("feedback-owner", saved.CreatedBy);
    }

    [Fact]
    public async Task Feedback_CommentOverLimitReturnsBadRequest()
    {
        var request = await SeedRequest("feedback-owner", RequestState.Closed);
        var response = await _factory.CreateClient().AsRequester("feedback-owner").PostAsJsonAsync(
            $"/api/requests/{request.Id}/feedback", new SubmitFeedbackDto { Rating = 3, Comment = new string('x', 2001) });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private async Task<Request> SeedRequest(string requesterId, RequestState state)
    {
        using var db = _factory.CreateDbContext();
        var request = MakeRequest(requesterId, $"{state}-{Guid.NewGuid()}", state);
        db.Requests.Add(request);
        await db.SaveChangesAsync();
        return request;
    }

    private static Request MakeRequest(string requesterId, string title, RequestState state = RequestState.Open, DateTime? createdAt = null) => new()
    {
        CategoryId = 1,
        Title = title,
        Description = "Description",
        RequesterId = requesterId,
        State = state,
        CreatedAt = createdAt ?? DateTime.UtcNow
    };
}
