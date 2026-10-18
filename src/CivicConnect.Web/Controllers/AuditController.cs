using System;
using System.Linq;
using System.Threading.Tasks;
using CivicConnect.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CivicConnect.Web.Controllers;

// NFR-020 (Issue #35): append-only audit trail reads. Manager only, no update/delete path.
[ApiController]
[Route("api/audit")]
public class AuditController : ControllerBase
{
    private readonly CivicConnectDbContext _context;

    public AuditController(CivicConnectDbContext context)
    {
        _context = context;
    }

    // GET api/audit?entityType=Request&entityId=1
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] string? entityType,
        [FromQuery] int? entityId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        if (!User.IsInRole("ADMIN") && !User.IsInRole("MANAGER"))
        {
            return Forbid();
        }

        if (page < 1 || pageSize < 1 || pageSize > 100)
        {
            return BadRequest("Page must be at least 1 and pageSize must be between 1 and 100.");
        }

        var query = _context.AuditLogs.AsNoTracking().AsQueryable();

        if (!string.IsNullOrEmpty(entityType))
        {
            query = query.Where(a => a.EntityType == entityType);
        }

        if (entityId.HasValue)
        {
            query = query.Where(a => a.EntityId == entityId.Value);
        }

        var totalCount = await query.CountAsync();
        var logs = await query
            .OrderByDescending(a => a.Timestamp)
            .ThenByDescending(a => a.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new PagedResultDto<Domain.Entities.AuditLog>
        {
            Items = logs,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        });
    }
}
