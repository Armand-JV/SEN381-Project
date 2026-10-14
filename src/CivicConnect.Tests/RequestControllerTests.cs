using System;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Linq;
using System.Threading.Tasks;
using CivicConnect.Domain.Entities;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using CivicConnect.Domain.Enums;
using CivicConnect.Web.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
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
        var db = _factory.CreateDbContext();
        var category = new Category { Name = "TestCategory" };
        db.Categories.Add(category);
        await db.SaveChangesAsync();

        var request = new Request
        {
            CategoryId = category.Id,
            Title = "Title",
            Description = "Desc",
            State = RequestState.Closed,
            RequesterId = "alice"
        };
        db.Requests.Add(request);
        await db.SaveChangesAsync();

        var response = await AuthenticateAs("alice").PutAsJsonAsync(
            $"/api/requests/{request.Id}/state",
            new UpdateStateDto { NewState = RequestState.Open, Reason = "Reopen" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateState_ValidTransition_ReturnsOkAndPersistsHistory()
    {
        var db = _factory.CreateDbContext();
        var category = new Category { Name = "TestCategory" };
        db.Categories.Add(category);
        await db.SaveChangesAsync();

        var request = new Request
        {
            CategoryId = category.Id,
            Title = "Open request",
            Description = "Description",
            State = RequestState.Open,
            RequesterId = "alice"
        };
        db.Requests.Add(request);
        await db.SaveChangesAsync();

        var response = await AuthenticateAs("alice").PutAsJsonAsync(
            $"/api/requests/{request.Id}/state",
            new UpdateStateDto { NewState = RequestState.InProgress, Reason = "Work started" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var verificationDb = _factory.CreateDbContext();
        Assert.Equal(RequestState.InProgress, verificationDb.Requests.Single(item => item.Id == request.Id).State);
        var history = Assert.Single(verificationDb.StatusHistories.Where(item => item.RequestId == request.Id));
        Assert.Equal(RequestState.Open, history.PreviousState);
        Assert.Equal(RequestState.InProgress, history.NewState);
    }

    [Fact]
    public async Task UpdateState_ObserverThrows_ReturnsOkAndPreservesStateChange()
    {
        var logObserverLogger = new Mock<ILogger<CivicConnect.Application.Patterns.Observer.LogRequestStateObserver>>();
        using var observerFactory = new TestWebApplicationFactoryWithThrowingObserver(logObserverLogger.Object);
        observerFactory.ResetDatabase();

        var db = observerFactory.CreateDbContext();
        var category = new Category { Name = "TestCategory" };
        db.Categories.Add(category);
        await db.SaveChangesAsync();

        var request = new Request
        {
            CategoryId = category.Id,
            Title = "Observer failure",
            Description = "Description",
            State = RequestState.Open,
            RequesterId = "alice"
        };
        db.Requests.Add(request);
        await db.SaveChangesAsync();

        var client = observerFactory.CreateClient();
        client.DefaultRequestHeaders.Remove("Authorization");
        client.DefaultRequestHeaders.Add("Authorization", BuildBearerToken("alice"));

        var response = await client.PutAsJsonAsync(
            $"/api/requests/{request.Id}/state",
            new UpdateStateDto { NewState = RequestState.InProgress, Reason = "Work started" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var verificationDb = observerFactory.CreateDbContext();
        Assert.Equal(RequestState.InProgress, verificationDb.Requests.Single(item => item.Id == request.Id).State);
        Assert.Single(verificationDb.StatusHistories.Where(item => item.RequestId == request.Id));
        VerifyLog(logObserverLogger, LogLevel.Information, $"Request {request.Id} changed state from Open to InProgress");
    }

    [Fact]
    public void Program_Registration_IncludesConcreteObserver()
    {
        using var scope = _factory.Services.CreateScope();
        var observers = scope.ServiceProvider.GetServices<CivicConnect.Application.Patterns.Observer.IRequestStateObserver>().ToList();
        var notifier = scope.ServiceProvider.GetRequiredService<CivicConnect.Application.Patterns.Observer.RequestStateNotifier>();

        Assert.Contains(observers, item => item is CivicConnect.Application.Patterns.Observer.LogRequestStateObserver);
        Assert.NotNull(notifier);
    }

    private System.Net.Http.HttpClient AuthenticateAs(string userId)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Remove("Authorization");
        client.DefaultRequestHeaders.Add("Authorization", BuildBearerToken(userId));
        return client;
    }

    private static string BuildBearerToken(string userId)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestWebApplicationFactory.JwtSecret));
        var token = new JwtSecurityToken(
            issuer: TestWebApplicationFactory.JwtIssuer,
            audience: TestWebApplicationFactory.JwtIssuer,
            claims: new[] { new Claim(ClaimTypes.NameIdentifier, userId), new Claim(ClaimTypes.Name, userId) },
            expires: DateTime.UtcNow.AddMinutes(5),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
        return "Bearer " + new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static void VerifyLog<T>(Mock<ILogger<T>> logger, LogLevel level, string message)
    {
        logger.Verify(log => log.Log(
            level,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((value, _) => value!.ToString()!.Contains(message)),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    private sealed class TestWebApplicationFactoryWithThrowingObserver : TestWebApplicationFactory
    {
        private readonly ILogger<CivicConnect.Application.Patterns.Observer.LogRequestStateObserver> _logger;

        public TestWebApplicationFactoryWithThrowingObserver(
            ILogger<CivicConnect.Application.Patterns.Observer.LogRequestStateObserver> logger)
        {
            _logger = logger;
        }

        protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);
            builder.ConfigureServices(services =>
            {
                services.AddSingleton(_logger);
                services.Insert(0, ServiceDescriptor.Scoped<CivicConnect.Application.Patterns.Observer.IRequestStateObserver, ThrowingRequestStateObserver>());
            });
        }
    }

    private sealed class ThrowingRequestStateObserver : CivicConnect.Application.Patterns.Observer.IRequestStateObserver
    {
        public Task OnStateChangedAsync(Request request, RequestState oldState, RequestState newState) =>
            Task.FromException(new InvalidOperationException("Observer failure"));
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
    public async Task CreateRequest_UsesAuthenticatedActorInsteadOfBodyRequesterId()
    {
        var response = await _factory.CreateClient().AsRequester("requester-123")
            .PostAsJsonAsync(
                "/api/requests",
                new
                {
                    id = 3,
                    title = "Authenticated request",
                    description = "Created by the authenticated requester.",
                    categoryId = 1,
                    requesterId = "spoofed-user",
                    state = RequestState.Closed
                });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var createdRequest = await response.Content.ReadFromJsonAsync<Request>();
        Assert.NotNull(createdRequest);
        Assert.Equal("requester-123", createdRequest.RequesterId);
        Assert.Equal(RequestState.Open, createdRequest.State);
        Assert.Equal("requester-123", createdRequest.History.Single().ChangedBy);
    }

    [Fact]
    public async Task CreateRequest_AnonymousCallerReturnsUnauthorized()
    {
        var response = await _factory.CreateClient()
            .PostAsJsonAsync(
                "/api/requests",
                new CreateRequestDto
                {
                    Title = "Unauthenticated request",
                    Description = "This request should not be created.",
                    CategoryId = 1
                });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetRequest_SeededRequestIncludesComment()
    {
        var request = await _factory.CreateClient().AsRequester("requester-2")
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

    [Fact]
    public async Task AssignRequest_Requester_ReturnsForbiddenAndLeavesRequestOpen()
    {
        var db = _factory.CreateDbContext();
        var request = new Request
        {
            CategoryId = 1,
            Title = "Protected assignment",
            Description = "Description",
            RequesterId = "requester-1",
            State = RequestState.Open,
            CreatedAt = DateTime.UtcNow
        };
        db.Requests.Add(request);
        await db.SaveChangesAsync();

        var response = await _factory.CreateClient().AsRequester("requester-1").PutAsJsonAsync(
            $"/api/requests/{request.Id}/assign",
            new AssignRequestDto { AssigneeId = "staff-1" });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal(RequestState.Open, db.Requests.Single(item => item.Id == request.Id).State);
        Assert.Null(db.Requests.Single(item => item.Id == request.Id).AssigneeId);
        Assert.Empty(db.StatusHistories.Where(item => item.RequestId == request.Id));
        Assert.Empty(db.AuditEntries.Where(item => item.RequestId == request.Id));
    }

    [Fact]
    public async Task UpdateState_ValidTransition_PersistsOneHistoryRowWithActor()
    {
        var db = _factory.CreateDbContext();
        var request = new Request
        {
            CategoryId = 1,
            Title = "Assigned request",
            Description = "Description",
            RequesterId = "requester-1",
            AssigneeId = "staff-2",
            State = RequestState.Assigned,
            CreatedAt = DateTime.UtcNow
        };
        db.Requests.Add(request);
        await db.SaveChangesAsync();

        var response = await _factory.CreateClient().AsStaff("staff-2").PutAsJsonAsync(
            $"/api/requests/{request.Id}/state",
            new UpdateStateDto { NewState = RequestState.InProgress, Reason = "Work started" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var verificationDb = _factory.CreateDbContext();
        Assert.Equal(RequestState.InProgress, verificationDb.Requests.Single(item => item.Id == request.Id).State);
        var history = Assert.Single(verificationDb.StatusHistories.Where(item => item.RequestId == request.Id));
        Assert.Equal(RequestState.Assigned, history.PreviousState);
        Assert.Equal(RequestState.InProgress, history.NewState);
        Assert.Equal("staff-2", history.ChangedBy);
    }

    [Fact]
    public async Task UpdateState_ReasonRequiredWithoutReason_ReturnsBadRequestAndLeavesRequestOpen()
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
            new UpdateStateDto { NewState = RequestState.Rejected, Reason = " " });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(RequestState.Open, db.Requests.Single(item => item.Id == request.Id).State);
        Assert.Empty(db.StatusHistories.Where(item => item.RequestId == request.Id));
    }

    [Fact]
    public async Task AddComment_RequesterPublicComment_PersistsAuthenticatedActor()
    {
        var response = await _factory.CreateClient().AsRequester("requester-2").PostAsJsonAsync(
            "/api/requests/2/comments",
            new { text = "More information from requester.", isInternal = false });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var comment = await response.Content.ReadFromJsonAsync<Comment>();
        Assert.NotNull(comment);
        Assert.Equal("requester-2", comment.CreatedBy);
        Assert.False(comment.IsInternal);

        var db = _factory.CreateDbContext();
        Assert.Contains(db.Comments, item =>
            item.RequestId == 2 &&
            item.Text == "More information from requester." &&
            item.CreatedBy == "requester-2" &&
            !item.IsInternal);
    }

    [Fact]
    public async Task AddComment_RequesterInternalComment_ReturnsForbiddenAndPersistsNothing()
    {
        var response = await _factory.CreateClient().AsRequester("requester-2").PostAsJsonAsync(
            "/api/requests/2/comments",
            new { text = "Requester cannot make this internal.", isInternal = true });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.DoesNotContain(
            _factory.CreateDbContext().Comments,
            item => item.Text == "Requester cannot make this internal.");
    }

    [Fact]
    public async Task GetRequest_RequesterCannotSeeInternalComments()
    {
        var db = _factory.CreateDbContext();
        db.Comments.Add(new Comment
        {
            RequestId = 2,
            Text = "Internal staff note",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "staff-1",
            IsInternal = true
        });
        await db.SaveChangesAsync();

        var request = await _factory.CreateClient().AsRequester("requester-2")
            .GetFromJsonAsync<Request>("/api/requests/2");

        Assert.NotNull(request);
        Assert.DoesNotContain(request.Comments, item => item.IsInternal);
        Assert.Contains(request.Comments, item => !item.IsInternal);
    }

    [Fact]
    public async Task GetRequest_StaffCanSeeInternalComments()
    {
        var db = _factory.CreateDbContext();
        db.Comments.Add(new Comment
        {
            RequestId = 2,
            Text = "Internal staff note",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "staff-1",
            IsInternal = true
        });
        await db.SaveChangesAsync();

        var request = await _factory.CreateClient().AsStaff("staff-1")
            .GetFromJsonAsync<Request>("/api/requests/2");

        Assert.NotNull(request);
        Assert.Contains(request.Comments, item => item.IsInternal && item.Text == "Internal staff note");
    }

    [Fact]
    public async Task AddComment_StaffInternalComment_HiddenFromRequester()
    {
        var response = await _factory.CreateClient().AsStaff("staff-1").PostAsJsonAsync(
            "/api/requests/2/comments",
            new { text = "Internal review", isInternal = true, createdBy = "spoofed-user" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<Comment>();
        Assert.NotNull(created);
        Assert.True(created.IsInternal);
        Assert.Equal("staff-1", created.CreatedBy);

        var request = await _factory.CreateClient().AsRequester("requester-2")
            .GetFromJsonAsync<Request>("/api/requests/2");
        Assert.NotNull(request);
        Assert.DoesNotContain(request.Comments, item => item.Id == created.Id);
        Assert.Contains(_factory.CreateDbContext().Comments, item => item.Id == created.Id);
    }

    [Fact]
    public async Task AddComment_OtherRequester_ReturnsForbiddenAndPersistsNothing()
    {
        var response = await _factory.CreateClient().AsRequester("requester-1").PostAsJsonAsync(
            "/api/requests/2/comments",
            new { text = "Not my request", isInternal = false });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.DoesNotContain(_factory.CreateDbContext().Comments, item => item.Text == "Not my request");
    }

    [Fact]
    public async Task AddComment_BlankText_ReturnsBadRequestAndPersistsNothing()
    {
        var response = await _factory.CreateClient().AsRequester("requester-2").PostAsJsonAsync(
            "/api/requests/2/comments",
            new { text = "   ", isInternal = false });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Single(_factory.CreateDbContext().Comments);
    }

    [Fact]
    public async Task GetRequest_Anonymous_ReturnsUnauthorized()
    {
        var response = await _factory.CreateClient().GetAsync("/api/requests/2");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AddComment_Anonymous_ReturnsUnauthorized()
    {
        var response = await _factory.CreateClient().PostAsJsonAsync(
            "/api/requests/2/comments",
            new { text = "Anonymous comment", isInternal = false });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Single(_factory.CreateDbContext().Comments);
    }

    [Fact]
    public async Task GetRequest_OtherRequester_ReturnsForbidden()
    {
        var response = await _factory.CreateClient().AsRequester("requester-1")
            .GetAsync("/api/requests/2");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
