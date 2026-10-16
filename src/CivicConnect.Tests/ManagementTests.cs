using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CivicConnect.Domain.Enums;
using CivicConnect.Tests.Builders;
using CivicConnect.Web.Controllers;
using CivicConnect.Web.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CivicConnect.Tests;

// FR-015 (Issue #33) stalled oversight + FR-016 (Issue #34) category administration.
public class ManagementTests : IDisposable
{
    private readonly CivicConnectDbContext _db;

    public ManagementTests()
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

    private ManagementController ControllerAs(params string[] roles)
    {
        var claims = new List<Claim> { new(ClaimTypes.Name, "boss") };
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        return new ManagementController(_db)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"))
                }
            }
        };
    }

    [Fact]
    public async Task Stalled_ReturnsOnlyOldOpenAndAssigned_StalestFirst()
    {
        var old = DateTime.UtcNow.AddDays(-30);
        var fresh = DateTime.UtcNow;
        _db.Requests.Add(new RequestBuilder().WithState(RequestState.Open).WithCreatedAt(old).Build());
        _db.Requests.Add(new RequestBuilder().WithState(RequestState.Assigned).WithCreatedAt(old.AddDays(-1)).Build());
        _db.Requests.Add(new RequestBuilder().WithState(RequestState.Open).WithCreatedAt(fresh).Build());
        _db.Requests.Add(new RequestBuilder().WithState(RequestState.Resolved).WithCreatedAt(old).Build());
        await _db.SaveChangesAsync();

        var result = Assert.IsType<OkObjectResult>(
            await ControllerAs("MANAGER").GetStalled(7));
        var items = Assert.IsAssignableFrom<IEnumerable<StalledRequestDto>>(result.Value).ToList();

        Assert.Equal(2, items.Count);
        Assert.True(items[0].CreatedAt <= items[1].CreatedAt);
        Assert.All(items, item => Assert.True(
            item.State is RequestState.Open or RequestState.Assigned));
    }

    [Fact]
    public async Task Stalled_NonManagerGetsForbid_NegativeDaysGetsBadRequest()
    {
        Assert.IsType<ForbidResult>(await ControllerAs("REQUESTER").GetStalled(7));
        Assert.IsType<BadRequestObjectResult>(await ControllerAs("MANAGER").GetStalled(-1));
    }

    [Fact]
    public async Task Reassign_OpenMovesToAssigned_WithHistoryAndAudit()
    {
        var request = new RequestBuilder().WithState(RequestState.Open).Build();
        _db.Requests.Add(request);
        await _db.SaveChangesAsync();

        var result = Assert.IsType<OkObjectResult>(await ControllerAs("MANAGER").Reassign(
            request.Id, new ReassignRequestDto { AssigneeId = "staff-9" }));

        Assert.Equal(RequestState.Assigned, request.State);
        Assert.Equal("staff-9", request.AssigneeId);
        Assert.Single(_db.StatusHistories.Where(h => h.RequestId == request.Id));
        var audit = Assert.Single(_db.AuditLogs.Where(a => a.EntityId == request.Id));
        Assert.Equal("Reassigned", audit.Action);
        Assert.Equal("unassigned", audit.BeforeSummary);
        Assert.Equal("staff-9", audit.AfterSummary);
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Reassign_AssignedKeepsState_ButUpdatesAssignee()
    {
        var request = new RequestBuilder().WithState(RequestState.Assigned).Build();
        request.AssigneeId = "staff-1";
        _db.Requests.Add(request);
        await _db.SaveChangesAsync();

        var result = await ControllerAs("ADMIN").Reassign(
            request.Id, new ReassignRequestDto { AssigneeId = "staff-2" });

        Assert.IsType<OkObjectResult>(result);
        Assert.Equal(RequestState.Assigned, request.State);
        Assert.Equal("staff-2", request.AssigneeId);
    }

    [Fact]
    public async Task Reassign_ForbiddenForRequester_NotFoundForUnknown()
    {
        Assert.IsType<ForbidResult>(await ControllerAs("REQUESTER").Reassign(
            1, new ReassignRequestDto { AssigneeId = "x" }));
        Assert.IsType<NotFoundResult>(await ControllerAs("MANAGER").Reassign(
            9999, new ReassignRequestDto { AssigneeId = "x" }));
        Assert.IsType<BadRequestObjectResult>(await ControllerAs("MANAGER").Reassign(
            1, new ReassignRequestDto { AssigneeId = "  " }));
    }

    [Fact]
    public async Task CreateCategory_DuplicateNameGetsConflict()
    {
        var created = Assert.IsType<CreatedResult>(await ControllerAs("MANAGER").CreateCategory(
            new CategoryWriteDto { Name = "Bridges", Description = "d" }));
        Assert.NotNull(created.Location);

        Assert.IsType<ConflictObjectResult>(await ControllerAs("MANAGER").CreateCategory(
            new CategoryWriteDto { Name = "Bridges" }));
        Assert.IsType<BadRequestObjectResult>(await ControllerAs("MANAGER").CreateCategory(
            new CategoryWriteDto { Name = "  " }));
        Assert.IsType<ForbidResult>(await ControllerAs("REQUESTER").CreateCategory(
            new CategoryWriteDto { Name = "Other" }));
    }

    [Fact]
    public async Task RenameCategory_UpdatesName_UnknownGetsNotFound()
    {
        _db.Categories.Add(new Domain.Entities.Category { Name = "Old", Description = "d" });
        await _db.SaveChangesAsync();
        var id = _db.Categories.Single(c => c.Name == "Old").Id;

        var result = Assert.IsType<OkObjectResult>(await ControllerAs("MANAGER").RenameCategory(
            id, new CategoryWriteDto { Name = "New", Description = "nd" }));
        Assert.Equal("New", ((Domain.Entities.Category)result.Value!).Name);

        Assert.IsType<NotFoundResult>(await ControllerAs("MANAGER").RenameCategory(
            9999, new CategoryWriteDto { Name = "New" }));
    }
}
