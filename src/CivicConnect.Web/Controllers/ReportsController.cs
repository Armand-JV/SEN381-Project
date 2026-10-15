using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CivicConnect.Domain.Enums;
using CivicConnect.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CivicConnect.Web.Controllers;

// FR-014 (Issue #32): management dashboard aggregation, Admin or Manager only.
[ApiController]
[Route("api/reports")]
public class ReportsController : ControllerBase
{
    private readonly CivicConnectDbContext _context;

    public ReportsController(CivicConnectDbContext context)
    {
        _context = context;
    }

    // GET api/reports/requests-summary?state=&categoryId=&assigneeId=&from=&to=
    // from/to filter CreatedAt. Shaped to use the M3-DATA-01 (#36) indexes.
    [HttpGet("requests-summary")]
    [Authorize]
    public async Task<IActionResult> GetRequestsSummary(
        [FromQuery] RequestState? state,
        [FromQuery] int? categoryId,
        [FromQuery] string? assigneeId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        if (!User.IsInRole("ADMIN") && !User.IsInRole("MANAGER"))
        {
            return Forbid();
        }

        var query = _context.Requests.AsNoTracking().AsQueryable();

        if (state.HasValue)
        {
            query = query.Where(r => r.State == state.Value);
        }

        if (categoryId.HasValue)
        {
            query = query.Where(r => r.CategoryId == categoryId.Value);
        }

        if (!string.IsNullOrEmpty(assigneeId))
        {
            query = query.Where(r => r.AssigneeId == assigneeId);
        }

        if (from.HasValue)
        {
            query = query.Where(r => r.CreatedAt >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(r => r.CreatedAt <= to.Value);
        }

        var total = await query.CountAsync();

        var byState = await query
            .GroupBy(r => r.State)
            .Select(g => new { State = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.State.ToString(), x => x.Count);

        var byCategory = await query
            .GroupBy(r => r.CategoryId)
            .Select(g => new { CategoryId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.CategoryId, x => x.Count);

        return Ok(new RequestsSummaryDto
        {
            Total = total,
            ByState = byState,
            ByCategory = byCategory
        });
    }
}

public class RequestsSummaryDto
{
    public int Total { get; set; }
    public Dictionary<string, int> ByState { get; set; } = new();
    public Dictionary<int, int> ByCategory { get; set; } = new();
}
