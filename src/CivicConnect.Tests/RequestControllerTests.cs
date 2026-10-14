using System;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
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
}
