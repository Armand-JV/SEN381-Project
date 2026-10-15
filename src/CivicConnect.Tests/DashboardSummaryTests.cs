using System;
using System.Collections.Generic;
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

// TC-U-06 (FR-014, Issue #32): dashboard aggregation counts reconcile with the data.
// Pure unit test: controller directly over an InMemory DbContext with a stub principal.
// (The shared TestWebApplicationFactory is broken on main for all suites —
// Npgsql vs InMemory provider conflict, fails identically for RequestControllerTests —
// so this test deliberately avoids it. That harness fix belongs to P2 / M3-TEST-00 follow-up.)
public class DashboardSummaryTests : IDisposable
{
    private readonly CivicConnectDbContext _db;

    public DashboardSummaryTests()
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

    private ReportsController ControllerAs(params string[] roles)
    {
        var claims = new List<Claim> { new(ClaimTypes.Name, "boss") };
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        return new ReportsController(_db)
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

    private async Task SeedAsync()
    {
        var now = DateTime.UtcNow;
        var r1 = new RequestBuilder().WithCategoryId(1).WithState(RequestState.Open).WithRequesterId("u1").WithCreatedAt(now).Build();
        var r2 = new RequestBuilder().WithCategoryId(1).WithState(RequestState.Resolved).WithRequesterId("u2").WithCreatedAt(now).Build();
        r2.AssigneeId = "staff1";
        var r3 = new RequestBuilder().WithCategoryId(2).WithState(RequestState.Open).WithRequesterId("u1").WithCreatedAt(now).Build();
        r3.AssigneeId = "staff1";

        _db.Requests.AddRange(r1, r2, r3);
        await _db.SaveChangesAsync();
    }

    private static RequestsSummaryDto OkBody(IActionResult result)
    {
        var ok = Assert.IsType<OkObjectResult>(result);
        return Assert.IsType<RequestsSummaryDto>(ok.Value);
    }

    [Fact]
    public async Task Summary_CountsReconcile_WithUnderlyingData()
    {
        await SeedAsync();
        var controller = ControllerAs("ADMIN");

        var summary = OkBody(await controller.GetRequestsSummary(null, null, null, null, null));

        Assert.Equal(3, summary.Total);
        Assert.Equal(2, summary.ByState["Open"]);
        Assert.Equal(1, summary.ByState["Resolved"]);
        Assert.Equal(2, summary.ByCategory[1]);
        Assert.Equal(1, summary.ByCategory[2]);
    }

    [Fact]
    public async Task Summary_FiltersCombineCorrectly()
    {
        await SeedAsync();
        var controller = ControllerAs("MANAGER");

        var byState = OkBody(await controller.GetRequestsSummary(RequestState.Open, null, null, null, null));
        Assert.Equal(2, byState.Total);

        var byCategory = OkBody(await controller.GetRequestsSummary(null, 1, null, null, null));
        Assert.Equal(2, byCategory.Total);

        var byAssignee = OkBody(await controller.GetRequestsSummary(null, null, "staff1", null, null));
        Assert.Equal(2, byAssignee.Total);
    }

    [Fact]
    public async Task Summary_NonManagerGetsForbid()
    {
        await SeedAsync();
        var controller = ControllerAs("REQUESTER");

        Assert.IsType<ForbidResult>(await controller.GetRequestsSummary(null, null, null, null, null));
    }
}
