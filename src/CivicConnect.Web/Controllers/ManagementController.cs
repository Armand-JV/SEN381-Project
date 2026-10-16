using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CivicConnect.Application.Policies;
using CivicConnect.Domain.Entities;
using CivicConnect.Domain.Enums;
using CivicConnect.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CivicConnect.Web.Controllers;

// FR-015 (Issue #33) stalled oversight + FR-016 (Issue #34) category administration.
// ADMIN or MANAGER only. See docs/P3-Decisions.md for the accepted criteria.
[ApiController]
[Route("api/management")]
public class ManagementController : ControllerBase
{
    private readonly CivicConnectDbContext _context;

    public ManagementController(CivicConnectDbContext context)
    {
        _context = context;
    }

    private bool IsManager() => User.IsInRole("ADMIN") || User.IsInRole("MANAGER");

    private string Actor() =>
        User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
        ?? User.Identity?.Name ?? "Anonymous";

    // GET api/management/stalled?olderThanDays=7 — plain age filter, no overdue semantics.
    [HttpGet("stalled")]
    [Authorize]
    public async Task<IActionResult> GetStalled([FromQuery] int olderThanDays = 7)
    {
        if (!IsManager()) return Forbid();
        if (olderThanDays < 0) return BadRequest("olderThanDays must be zero or positive.");

        var cutoff = DateTime.UtcNow.AddDays(-olderThanDays);
        var now = DateTime.UtcNow;

        var items = await _context.Requests
            .AsNoTracking()
            .Where(r => (r.State == RequestState.Open || r.State == RequestState.Assigned)
                && r.CreatedAt <= cutoff)
            .OrderBy(r => r.CreatedAt)
            .Select(r => new StalledRequestDto
            {
                Id = r.Id,
                Title = r.Title,
                State = r.State,
                CreatedAt = r.CreatedAt,
                AssigneeId = r.AssigneeId,
                DaysOld = (int)(now - r.CreatedAt).TotalDays
            })
            .ToListAsync();

        return Ok(items);
    }

    // PUT api/management/requests/{id}/reassign.
    [HttpPut("requests/{id}/reassign")]
    [Authorize]
    public async Task<IActionResult> Reassign(int id, [FromBody] ReassignRequestDto dto)
    {
        if (!IsManager()) return Forbid();
        if (string.IsNullOrWhiteSpace(dto.AssigneeId)) return BadRequest("AssigneeId is required.");

        var request = await _context.Requests.FindAsync(id);
        if (request == null) return NotFound();

        var actor = Actor();
        var oldState = request.State;
        var oldAssignee = request.AssigneeId;
        var assignee = dto.AssigneeId.Trim();
        var now = DateTime.UtcNow;

        if (request.State == RequestState.Open)
        {
            if (!RequestStateTransitionPolicy.IsAllowed(RequestState.Open, RequestState.Assigned))
            {
                return BadRequest("Request cannot transition to Assigned.");
            }

            request.AssigneeId = assignee;
            request.State = RequestState.Assigned;
        }
        else
        {
            request.AssigneeId = assignee;
        }

        _context.StatusHistories.Add(new StatusHistory
        {
            RequestId = request.Id,
            PreviousState = oldState,
            NewState = request.State,
            ChangedAt = now,
            ChangedBy = actor,
            Reason = string.IsNullOrWhiteSpace(dto.Reason) ? $"Reassigned to {assignee}." : dto.Reason
        });

        _context.AuditLogs.Add(new AuditLog
        {
            Actor = actor,
            Action = "Reassigned",
            EntityType = nameof(Request),
            EntityId = request.Id,
            BeforeSummary = oldAssignee ?? "unassigned",
            AfterSummary = assignee,
            Timestamp = now,
            CorrelationId = Guid.NewGuid().ToString()
        });

        await _context.SaveChangesAsync();
        return Ok(request);
    }

    // POST api/management/categories.
    [HttpPost("categories")]
    [Authorize]
    public async Task<IActionResult> CreateCategory([FromBody] CategoryWriteDto dto)
    {
        if (!IsManager()) return Forbid();
        var name = dto.Name?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(name)) return BadRequest("Name is required.");

        if (await _context.Categories.AnyAsync(c => c.Name == name))
        {
            return Conflict("A category with this name already exists.");
        }

        var category = new Category { Name = name, Description = dto.Description?.Trim() ?? string.Empty };
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
        return Created($"/api/management/categories/{category.Id}", category);
    }

    // PUT api/management/categories/{id}.
    [HttpPut("categories/{id}")]
    [Authorize]
    public async Task<IActionResult> RenameCategory(int id, [FromBody] CategoryWriteDto dto)
    {
        if (!IsManager()) return Forbid();
        var name = dto.Name?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(name)) return BadRequest("Name is required.");

        var category = await _context.Categories.FindAsync(id);
        if (category == null) return NotFound();

        if (await _context.Categories.AnyAsync(c => c.Id != id && c.Name == name))
        {
            return Conflict("A category with this name already exists.");
        }

        category.Name = name;
        category.Description = dto.Description?.Trim() ?? string.Empty;
        await _context.SaveChangesAsync();
        return Ok(category);
    }
}

public class StalledRequestDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public RequestState State { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? AssigneeId { get; set; }
    public int DaysOld { get; set; }
}

public class ReassignRequestDto
{
    public string AssigneeId { get; set; } = string.Empty;
    public string? Reason { get; set; }
}

public class CategoryWriteDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
}
