using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CivicConnect.Application.Patterns.Observer;
using CivicConnect.Application.Patterns.Strategy;
using CivicConnect.Application.Services;
using CivicConnect.Domain.Entities;
using CivicConnect.Domain.Enums;
using CivicConnect.Tests.Builders;
using CivicConnect.Web.Controllers;
using CivicConnect.Web.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CivicConnect.Tests;

// NFR-020 (Issue #35): each state change writes exactly one audit entry;
// audit reads are manager-only.
public class AuditLogTests : IDisposable
{
    private readonly CivicConnectDbContext _db;

    public AuditLogTests()
    {
        var options = new DbContextOptionsBuilder<CivicConnectDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new CivicConnectDbContext(options);
    }

    public void Dispose()
    {
        _db.Dispose();
        GC.SuppressFinalize(this);
    }

    private static void SetUser(ControllerBase controller, string name, params string[] roles)
    {
        var claims = new List<Claim> { new(ClaimTypes.Name, name) };
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"))
            }
        };
    }

    private RequestsController RequestsControllerAs(string name, params string[] roles)
    {
        var controller = new RequestsController(
            _db,
            new CategoryValidationContext(Enumerable.Empty<ICategoryValidationStrategy>()),
            new RequestStateNotifier(
                Enumerable.Empty<IRequestStateObserver>(),
                NullLogger<RequestStateNotifier>.Instance),
            new AccessControlService());
        SetUser(controller, name, roles);
        return controller;
    }

    [Fact]
    public async Task UpdateState_WritesExactlyOneAuditEntry_InSameTransaction()
    {
        var request = new RequestBuilder().WithState(RequestState.Open).Build();
        request.AssigneeId = "staff1";
        _db.Requests.Add(request);
        await _db.SaveChangesAsync();

        var controller = RequestsControllerAs("boss", "MANAGER");
        var result = await controller.UpdateState(
            request.Id, new UpdateStateDto { NewState = RequestState.Assigned, Reason = "Assigning" });

        Assert.IsType<OkObjectResult>(result);

        var entries = _db.AuditLogs.Where(a => a.EntityId == request.Id).ToList();
        var entry = Assert.Single(entries);
        Assert.Equal("boss", entry.Actor);
        Assert.Equal("StateChanged", entry.Action);
        Assert.Equal(nameof(Request), entry.EntityType);
        Assert.Equal(nameof(RequestState.Open), entry.BeforeSummary);
        Assert.Equal(nameof(RequestState.Assigned), entry.AfterSummary);
        Assert.NotNull(entry.CorrelationId);
    }

    [Fact]
    public async Task AuditRead_PaginatesFilteredEntries()
    {
        for (var index = 0; index < 3; index++)
        {
            _db.AuditLogs.Add(new AuditLog
            {
                Actor = "boss",
                Action = "StateChanged",
                EntityType = nameof(Request),
                EntityId = 1,
                Timestamp = DateTime.UtcNow.AddMinutes(index)
            });
        }
        _db.AuditLogs.Add(new AuditLog
        {
            Actor = "boss",
            Action = "Other",
            EntityType = "User",
            EntityId = 2,
            Timestamp = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();

        var controller = new AuditController(_db);
        SetUser(controller, "boss", "MANAGER");
        var result = Assert.IsType<OkObjectResult>(await controller.GetAuditLogs("Request", 1, page: 2, pageSize: 2));
        var page = Assert.IsType<PagedResultDto<AuditLog>>(result.Value);

        Assert.Equal(3, page.TotalCount);
        Assert.Equal(2, page.TotalPages);
        Assert.Single(page.Items);
        Assert.IsType<BadRequestObjectResult>(await controller.GetAuditLogs(null, null, page: 0));
    }

    [Fact]
    public async Task AuditRead_ManagerSeesEntries_RequesterGetsForbid()
    {
        _db.AuditLogs.Add(new AuditLog
        {
            Actor = "boss",
            Action = "StateChanged",
            EntityType = nameof(Request),
            EntityId = 1,
            Timestamp = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();

        var manager = new AuditController(_db);
        SetUser(manager, "boss", "MANAGER");
        var ok = Assert.IsType<OkObjectResult>(await manager.GetAuditLogs("Request", 1));
        var page = Assert.IsType<PagedResultDto<AuditLog>>(ok.Value);
        Assert.Single(page.Items);

        var requester = new AuditController(_db);
        SetUser(requester, "u1", "REQUESTER");
        Assert.IsType<ForbidResult>(await requester.GetAuditLogs(null, null));
    }
}
