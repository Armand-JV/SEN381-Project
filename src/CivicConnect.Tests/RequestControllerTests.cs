using System;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using CivicConnect.Domain.Entities;
using CivicConnect.Domain.Enums;
using CivicConnect.Web.Controllers;
using CivicConnect.Web.Data;
using Microsoft.EntityFrameworkCore;
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
        var client = _factory.CreateClient().AsStaff("staff1");
        var request = await SeedRequest(RequestState.Closed, requesterId: "user1", assigneeId: "staff1");

        var response = await client.PutAsJsonAsync($"/api/requests/{request.Id}/state",
            new UpdateStateDto { NewState = RequestState.Open, Reason = "Should fail" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var verificationScope = _factory.Services.CreateScope();
        var verificationDb = verificationScope.ServiceProvider.GetRequiredService<CivicConnectDbContext>();
        Assert.Equal(RequestState.Closed, verificationDb.Requests.Single(item => item.Id == request.Id).State);
        Assert.Empty(verificationDb.StatusHistories.Where(item => item.RequestId == request.Id));
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
        using var verificationScope = _factory.Services.CreateScope();
        var verificationDb = verificationScope.ServiceProvider.GetRequiredService<CivicConnectDbContext>();
        Assert.Equal(RequestState.Assigned, verificationDb.Requests.Single(item => item.Id == request.Id).State);
        Assert.Empty(verificationDb.StatusHistories.Where(item => item.RequestId == request.Id));
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

    [Fact]
    public async Task UpdateState_ValidTransition_ReturnsOkAndPersistsHistory()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CivicConnectDbContext>();
        var category = new Category { Name = "TestCategory" };
        db.Categories.Add(category);
        await db.SaveChangesAsync();

        var request = new Request
        {
            CategoryId = category.Id,
            Title = "Assigned request",
            Description = "Description",
            State = RequestState.Assigned,
            RequesterId = "alice",
            AssigneeId = "staff-1"
        };
        db.Requests.Add(request);
        await db.SaveChangesAsync();

        var response = await _factory.CreateClient().AsStaff("staff-1").PutAsJsonAsync(
            $"/api/requests/{request.Id}/state",
            new UpdateStateDto { NewState = RequestState.InProgress, Reason = "Work started" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var verificationScope = _factory.Services.CreateScope();
        var verificationDb = verificationScope.ServiceProvider.GetRequiredService<CivicConnectDbContext>();
        Assert.Equal(RequestState.InProgress, verificationDb.Requests.Single(item => item.Id == request.Id).State);
        var history = Assert.Single(verificationDb.StatusHistories.Where(item => item.RequestId == request.Id));
        Assert.Equal(RequestState.Assigned, history.PreviousState);
        Assert.Equal(RequestState.InProgress, history.NewState);
        Assert.Equal("staff-1", history.ChangedBy);
    }

    [Fact]
    public async Task UpdateState_ValidTransition_NotifiesObserver()
    {
        var observer = new Mock<CivicConnect.Application.Patterns.Observer.IRequestStateObserver>();
        using var observerFactory = new TestWebApplicationFactoryWithObserver(observer.Object);
        observerFactory.ResetDatabase();

        using var seedScope = observerFactory.Services.CreateScope();
        var seedDb = seedScope.ServiceProvider.GetRequiredService<CivicConnectDbContext>();
        var request = new Request
        {
            CategoryId = 1,
            Title = "Observer notification",
            Description = "Description",
            State = RequestState.Assigned,
            RequesterId = "requester-1",
            AssigneeId = "staff-1",
            CreatedAt = DateTime.UtcNow
        };
        seedDb.Requests.Add(request);
        await seedDb.SaveChangesAsync();

        var response = await observerFactory.CreateClient().AsStaff("staff-1").PutAsJsonAsync(
            $"/api/requests/{request.Id}/state",
            new UpdateStateDto { NewState = RequestState.InProgress, Reason = "Work started" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var verificationScope = observerFactory.Services.CreateScope();
        var verificationDb = verificationScope.ServiceProvider.GetRequiredService<CivicConnectDbContext>();
        Assert.Equal(RequestState.InProgress, verificationDb.Requests.Single(item => item.Id == request.Id).State);
        var history = Assert.Single(verificationDb.StatusHistories.Where(item => item.RequestId == request.Id));
        Assert.Equal(RequestState.Assigned, history.PreviousState);
        Assert.Equal(RequestState.InProgress, history.NewState);
        observer.Verify(item => item.OnStateChangedAsync(
            It.Is<Request>(changed => changed.Id == request.Id),
            RequestState.Assigned,
            RequestState.InProgress), Times.Once);
    }

    [Fact]
    public async Task UpdateState_ObserverThrows_ReturnsOkAndPreservesStateChange()
    {
        var logObserverLogger = new Mock<ILogger<CivicConnect.Application.Patterns.Observer.LogRequestStateObserver>>();
        logObserverLogger.Setup(log => log.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        using var observerFactory = new TestWebApplicationFactoryWithThrowingObserver(logObserverLogger.Object);
        observerFactory.ResetDatabase();

        using var observerScope = observerFactory.Services.CreateScope();
        var db = observerScope.ServiceProvider.GetRequiredService<CivicConnectDbContext>();
        var category = new Category { Name = "TestCategory" };
        db.Categories.Add(category);
        await db.SaveChangesAsync();

        var request = new Request
        {
            CategoryId = category.Id,
            Title = "Observer failure",
            Description = "Description",
            State = RequestState.Assigned,
            RequesterId = "alice",
            AssigneeId = "staff-1"
        };
        db.Requests.Add(request);
        await db.SaveChangesAsync();

        var client = observerFactory.CreateClient().AsStaff("staff-1");

        var response = await client.PutAsJsonAsync(
            $"/api/requests/{request.Id}/state",
            new UpdateStateDto { NewState = RequestState.InProgress, Reason = "Work started" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var observerVerificationScope = observerFactory.Services.CreateScope();
        var verificationDb = observerVerificationScope.ServiceProvider.GetRequiredService<CivicConnectDbContext>();
        Assert.Equal(RequestState.InProgress, verificationDb.Requests.Single(item => item.Id == request.Id).State);
        Assert.Single(verificationDb.StatusHistories.Where(item => item.RequestId == request.Id));
        VerifyLog(logObserverLogger, LogLevel.Information, $"Request {request.Id} changed state from Assigned to InProgress");
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

    private static void VerifyLog<T>(Mock<ILogger<T>> logger, LogLevel level, string message)
    {
#pragma warning disable CA1873
        logger.Verify(log => log.Log(
            level,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((value, _) => value!.ToString()!.Contains(message)),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
#pragma warning restore CA1873
    }

    private sealed class TestWebApplicationFactoryWithObserver : TestWebApplicationFactory
    {
        private readonly CivicConnect.Application.Patterns.Observer.IRequestStateObserver _observer;

        public TestWebApplicationFactoryWithObserver(CivicConnect.Application.Patterns.Observer.IRequestStateObserver observer)
        {
            _observer = observer;
        }

        protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);
            builder.ConfigureServices(services =>
                services.AddSingleton(_observer));
        }
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
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CivicConnectDbContext>();
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

        var response = await _factory.CreateClient().AsStaff("staff-a")
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

        var createdRequest = await response.Content.ReadFromJsonAsync<CreatedRequestDto>();
        Assert.NotNull(createdRequest);
        Assert.Equal("requester-123", createdRequest.RequesterId);
        Assert.Equal(RequestState.Open, createdRequest.State);
        using var db = _factory.CreateDbContext();
        var persisted = db.Requests
            .Include(item => item.History)
            .Single(item => item.Id == createdRequest.Id);
        Assert.Equal("requester-123", persisted.History.Single().ChangedBy);
    }

    [Fact]
    public async Task CreateRequest_ValidSubmission_ReturnsCreatedDtoAndPersistsInitialHistory()
    {
        var response = await _factory.CreateClient().AsRequester("requester-integration")
            .PostAsJsonAsync(
                "/api/requests",
                new CreateRequestDto
                {
                    Title = "Broken pavement",
                    Description = "Sidewalk crack near library.",
                    CategoryId = 1
                });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<CreatedRequestDto>();
        Assert.NotNull(created);
        Assert.Equal("Broken pavement", created.Title);
        Assert.Equal("Sidewalk crack near library.", created.Description);
        Assert.Equal(1, created.CategoryId);
        Assert.Equal("requester-integration", created.RequesterId);
        Assert.Equal(RequestState.Open, created.State);
        Assert.EndsWith($"/api/Requests/{created.Id}", response.Headers.Location?.OriginalString);

        using var verificationScope = _factory.Services.CreateScope();
        var verificationDb = verificationScope.ServiceProvider.GetRequiredService<CivicConnectDbContext>();
        var request = verificationDb.Requests.Single(item => item.Id == created.Id);
        Assert.Equal(RequestState.Open, request.State);
        var history = Assert.Single(verificationDb.StatusHistories.Where(item => item.RequestId == created.Id));
        Assert.Equal(RequestState.Open, history.PreviousState);
        Assert.Equal(RequestState.Open, history.NewState);
        Assert.Equal("requester-integration", history.ChangedBy);
        Assert.Equal("Initial submission", history.Reason);
    }

    [Fact]
    public async Task CreateRequest_UnknownCategory_ReturnsBadRequestAndPersistsNothing()
    {
        const string title = "Unknown category request";
        var response = await _factory.CreateClient().AsRequester("requester-integration")
            .PostAsJsonAsync(
                "/api/requests",
                new CreateRequestDto
                {
                    Title = title,
                    Description = "This must not be persisted.",
                    CategoryId = int.MaxValue
                });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var verificationScope = _factory.Services.CreateScope();
        var verificationDb = verificationScope.ServiceProvider.GetRequiredService<CivicConnectDbContext>();
        Assert.DoesNotContain(verificationDb.Requests, item => item.Title == title);
        Assert.DoesNotContain(verificationDb.StatusHistories, item => item.ChangedBy == "requester-integration");
    }

    [Fact]
    public async Task CreateRequest_AnonymousCallerReturnsUnauthorized()
    {
        const string title = "Unauthenticated request";
        var response = await _factory.CreateClient()
            .PostAsJsonAsync(
                "/api/requests",
                new CreateRequestDto
                {
                    Title = title,
                    Description = "This request should not be created.",
                    CategoryId = 1
                });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        using var verificationScope = _factory.Services.CreateScope();
        var verificationDb = verificationScope.ServiceProvider.GetRequiredService<CivicConnectDbContext>();
        Assert.DoesNotContain(verificationDb.Requests, item => item.Title == title);
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
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CivicConnectDbContext>();
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

        using var verificationScope = _factory.Services.CreateScope();
        var verificationDb = verificationScope.ServiceProvider.GetRequiredService<CivicConnectDbContext>();
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
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CivicConnectDbContext>();
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
        using var verificationScope = _factory.Services.CreateScope();
        var verificationDb = verificationScope.ServiceProvider.GetRequiredService<CivicConnectDbContext>();
        Assert.Equal(RequestState.Open, verificationDb.Requests.Single(item => item.Id == request.Id).State);
        Assert.Empty(verificationDb.StatusHistories.Where(item => item.RequestId == request.Id));
    }

    [Fact]
    public async Task AssignRequest_StaffOutsideAssignment_ReturnsForbiddenAndLeavesRequestUnchanged()
    {
        var request = await SeedRequest(
            RequestState.Open,
            requesterId: "requester-scope",
            assigneeId: "staff-owner");

        var response = await _factory.CreateClient().AsStaff("staff-outsider").PutAsJsonAsync(
            $"/api/requests/{request.Id}/assign",
            new AssignRequestDto { AssigneeId = "staff-next", Reason = "Reassign request" });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

        using var verificationScope = _factory.Services.CreateScope();
        var verificationDb = verificationScope.ServiceProvider.GetRequiredService<CivicConnectDbContext>();
        var persisted = verificationDb.Requests.Single(item => item.Id == request.Id);

        Assert.Equal(RequestState.Open, persisted.State);
        Assert.Equal("staff-owner", persisted.AssigneeId);
        Assert.Empty(verificationDb.StatusHistories.Where(item => item.RequestId == request.Id));
        Assert.Empty(verificationDb.AuditEntries.Where(item => item.RequestId == request.Id));
    }

    [Fact]
    public async Task UpdateState_StaffOutsideScope_ReturnsForbiddenAndLeavesRequestUnchanged()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CivicConnectDbContext>();
        var request = new Request
        {
            CategoryId = 1,
            Title = "Protected assignment",
            Description = "Description",
            RequesterId = "requester-1",
            AssigneeId = "staff-other",
            State = RequestState.Assigned,
            CreatedAt = DateTime.UtcNow
        };
        db.Requests.Add(request);
        await db.SaveChangesAsync();

        var response = await _factory.CreateClient().AsStaff("staff-1").PutAsJsonAsync(
            $"/api/requests/{request.Id}/state",
            new UpdateStateDto { NewState = RequestState.InProgress, Reason = "Should fail" });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        using var verificationScope = _factory.Services.CreateScope();
        var verificationDb = verificationScope.ServiceProvider.GetRequiredService<CivicConnectDbContext>();
        Assert.Equal(RequestState.Assigned, verificationDb.Requests.Single(item => item.Id == request.Id).State);
        Assert.Empty(verificationDb.StatusHistories.Where(item => item.RequestId == request.Id));
    }

    [Fact]
    public async Task AssignRequest_Requester_ReturnsForbiddenAndLeavesRequestOpen()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CivicConnectDbContext>();
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
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CivicConnectDbContext>();
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
        using var verificationScope = _factory.Services.CreateScope();
        var verificationDb = verificationScope.ServiceProvider.GetRequiredService<CivicConnectDbContext>();
        Assert.Equal(RequestState.InProgress, verificationDb.Requests.Single(item => item.Id == request.Id).State);
        var history = Assert.Single(verificationDb.StatusHistories.Where(item => item.RequestId == request.Id));
        Assert.Equal(RequestState.Assigned, history.PreviousState);
        Assert.Equal(RequestState.InProgress, history.NewState);
        Assert.Equal("staff-2", history.ChangedBy);
    }

    [Fact]
    public async Task UpdateState_ReasonRequiredWithoutReason_ReturnsBadRequestAndLeavesRequestOpen()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CivicConnectDbContext>();
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

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CivicConnectDbContext>();
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
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CivicConnectDbContext>();
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
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CivicConnectDbContext>();
        db.Comments.Add(new Comment
        {
            RequestId = 2,
            Text = "Internal staff note",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "staff-1",
            IsInternal = true
        });
        await db.SaveChangesAsync();

        using (var assignmentScope = _factory.Services.CreateScope())
        {
            var assignmentDb = assignmentScope.ServiceProvider.GetRequiredService<CivicConnectDbContext>();
            assignmentDb.Requests.Single(item => item.Id == 2).AssigneeId = "staff-1";
            await assignmentDb.SaveChangesAsync();
        }

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
        Assert.NotNull(_factory.CreateDbContext().Requests.Single(item => item.Id == 2));
    }

    [Fact]
    public async Task UpdateState_CreatesRequesterScopedNotification_ForStateChange()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CivicConnectDbContext>();
        var request = new Request
        {
            CategoryId = 1,
            Title = "Notification target",
            Description = "Description",
            RequesterId = "notification-requester",
            AssigneeId = "staff-1",
            State = RequestState.Assigned,
            CreatedAt = DateTime.UtcNow
        };
        db.Requests.Add(request);
        await db.SaveChangesAsync();

        var response = await _factory.CreateClient().AsStaff("staff-1").PutAsJsonAsync(
            $"/api/requests/{request.Id}/state",
            new UpdateStateDto { NewState = RequestState.InProgress, Reason = "Work started" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Notification persisted with RecipientId == requester and tied to the request.
        using var verifyScope = _factory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<CivicConnectDbContext>();
        var notification = Assert.Single(verifyDb.Notifications.Where(item => item.RequestId == request.Id));
        Assert.Equal("notification-requester", notification.RecipientId);
        Assert.Contains($"Request #{request.Id}", notification.Message);
    }

    [Fact]
    public async Task Notifications_RequesterSeesOwnUnreadListAndMarksRead()
    {
        // Seed a request whose state change will create a notification for the requester.
        using (var seedScope = _factory.Services.CreateScope())
        {
            var seedDb = seedScope.ServiceProvider.GetRequiredService<CivicConnectDbContext>();
            var request = new Request
            {
                CategoryId = 1,
                Title = "Notify me",
                Description = "Description",
                RequesterId = "notify-requester",
                AssigneeId = "staff-1",
                State = RequestState.Assigned,
                CreatedAt = DateTime.UtcNow
            };
            seedDb.Requests.Add(request);
            await seedDb.SaveChangesAsync();

            // Trigger the state change that emits the notification.
            await _factory.CreateClient().AsStaff("staff-1").PutAsJsonAsync(
                $"/api/requests/{request.Id}/state",
                new UpdateStateDto { NewState = RequestState.InProgress, Reason = "Start" });
        }

        // Requester reads their unread notifications.
        var requester = _factory.CreateClient().AsRequester("notify-requester");
        var unread = await requester.GetFromJsonAsync<PagedResultDto<CivicConnect.Web.Controllers.NotificationDto>>(
            "/api/notifications?unreadOnly=true");
        Assert.NotNull(unread);
        Assert.True(unread.TotalCount >= 1);
        var notification = unread.Items[0];

        // Requester cannot see another requester's notifications.
        var other = _factory.CreateClient().AsRequester("other-requester");
        var otherResponse = await other.GetAsync("/api/notifications?unreadOnly=true");
        Assert.Equal(HttpStatusCode.OK, otherResponse.StatusCode);
        var otherList = await otherResponse.Content.ReadFromJsonAsync<PagedResultDto<CivicConnect.Web.Controllers.NotificationDto>>();
        Assert.NotNull(otherList);
        Assert.DoesNotContain(otherList.Items, item => item.Id == notification.Id);

        // Mark read.
        var mark = await requester.PutAsync($"/api/notifications/{notification.Id}/read", null);
        Assert.Equal(HttpStatusCode.NoContent, mark.StatusCode);

        var after = await requester.GetFromJsonAsync<PagedResultDto<CivicConnect.Web.Controllers.NotificationDto>>(
            "/api/notifications?unreadOnly=true");
        Assert.NotNull(after);
        Assert.DoesNotContain(after.Items, item => item.Id == notification.Id);

        var all = await requester.GetFromJsonAsync<PagedResultDto<CivicConnect.Web.Controllers.NotificationDto>>(
            "/api/notifications");
        Assert.NotNull(all);
        Assert.Contains(all.Items, item => item.Id == notification.Id && item.ReadAt.HasValue);
    }
}
