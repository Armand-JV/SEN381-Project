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
        [FromQuery] int? entityId)
    {
        if (!User.IsInRole("ADMIN") && !User.IsInRole("MANAGER"))
        {
            return Forbid();
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

        var logs = await query
            .OrderByDescending(a => a.Timestamp)
            .Take(200)
            .ToListAsync();

        return Ok(logs);
    }
}
