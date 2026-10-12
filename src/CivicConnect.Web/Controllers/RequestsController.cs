using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using CivicConnect.Application.Policies;
using CivicConnect.Application.Patterns.Observer;
using CivicConnect.Application.Patterns.Strategy;
using CivicConnect.Domain.Entities;
using CivicConnect.Domain.Enums;
using CivicConnect.Web.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CivicConnect.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RequestsController : ControllerBase
{
    private readonly CivicConnectDbContext _context;
    private readonly CategoryValidationContext _validationContext;
    private readonly RequestStateNotifier _notifier;

    public RequestsController(
        CivicConnectDbContext context,
        CategoryValidationContext validationContext,
        RequestStateNotifier notifier)
    {
        _context = context;
        _validationContext = validationContext;
        _notifier = notifier;
    }

    [HttpPost]
    public async Task<IActionResult> CreateRequest([FromBody] CreateRequestDto dto)
    {
        var category = await _context.Categories.FindAsync(dto.CategoryId);
        if (category == null) return BadRequest("Invalid category.");

        var request = new Request
        {
            Title = dto.Title,
            Description = dto.Description,
            CategoryId = dto.CategoryId,
            CreatedAt = DateTime.UtcNow,
            RequesterId = User.Identity?.Name ?? "Anonymous",
            State = RequestState.Open
        };

        if (!_validationContext.Validate(request))
        {
            return BadRequest("Request validation failed for the selected category.");
        }

        _context.Requests.Add(request);
        
        // Initial state log
        request.History.Add(new StatusHistory
        {
            PreviousState = RequestState.Open,
            NewState = RequestState.Open,
            ChangedAt = DateTime.UtcNow,
            ChangedBy = User.Identity?.Name ?? "Anonymous",
            Reason = "Initial submission"
        });

        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetRequest), new { id = request.Id }, request);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetRequest(int id)
    {
        var request = await _context.Requests
            .Include(r => r.Category)
            .Include(r => r.Comments)
            .FirstOrDefaultAsync(r => r.Id == id);
        if (request == null) return NotFound();
        return Ok(request);
    }

    [HttpGet("/api/staff/requests")]
    [Authorize(Roles = "STAFF,MANAGER")]
    public async Task<IActionResult> GetStaffRequests(
        [FromQuery] RequestState? state = null,
        [FromQuery] int? categoryId = null,
        [FromQuery] string? assigneeId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100)
        {
            return BadRequest("Page must be at least 1 and pageSize must be between 1 and 100.");
        }

        var query = _context.Requests
            .AsNoTracking()
            .Include(request => request.Category)
            .AsQueryable();

        if (state.HasValue)
        {
            if (!Enum.IsDefined(state.Value))
            {
                return BadRequest("Invalid request state.");
            }

            query = query.Where(request => request.State == state.Value);
        }

        if (categoryId.HasValue)
        {
            query = query.Where(request => request.CategoryId == categoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(assigneeId))
        {
            query = query.Where(request => request.AssigneeId == assigneeId.Trim());
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(request => request.CreatedAt)
            .ThenByDescending(request => request.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(request => new StaffRequestListItemDto
            {
                Id = request.Id,
                Title = request.Title,
                Description = request.Description,
                CategoryId = request.CategoryId,
                CategoryName = request.Category!.Name,
                State = request.State,
                CreatedAt = request.CreatedAt,
                RequesterId = request.RequesterId,
                AssigneeId = request.AssigneeId
            })
            .ToListAsync();

        return Ok(new PagedResultDto<StaffRequestListItemDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        });
    }

    [HttpPut("/api/requests/{id}/assign")]
    [Authorize(Roles = "STAFF,MANAGER")]
    public async Task<IActionResult> AssignRequest(int id, [FromBody] AssignRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.AssigneeId))
        {
            return BadRequest("AssigneeId is required.");
        }

        var request = await _context.Requests.FindAsync(id);
        if (request == null) return NotFound();

        if (!RequestStateTransitionPolicy.IsAllowed(request.State, RequestState.Assigned))
        {
            return Conflict("Only open requests can be assigned.");
        }

        var actor = GetActor();
        var now = DateTime.UtcNow;
        var assigneeId = dto.AssigneeId.Trim();
        request.AssigneeId = assigneeId;
        request.State = RequestState.Assigned;

        _context.StatusHistories.Add(new StatusHistory
        {
            RequestId = request.Id,
            PreviousState = RequestState.Open,
            NewState = RequestState.Assigned,
            ChangedAt = now,
            ChangedBy = actor,
            Reason = dto.Reason
        });
        _context.AuditEntries.Add(new AuditEntry
        {
            RequestId = request.Id,
            Action = "RequestAssigned",
            ActorId = actor,
            CreatedAt = now,
            Details = $"Assigned to {assigneeId}."
        });

        await _context.SaveChangesAsync();
        await _notifier.NotifyStateChangedAsync(request, RequestState.Open, RequestState.Assigned);

        return Ok(request);
    }

    [HttpPut("{id}/state")]
    [Authorize(Roles = "STAFF,MANAGER")]
    public async Task<IActionResult> UpdateState(int id, [FromBody] UpdateStateDto dto)
    {
        var request = await _context.Requests.FindAsync(id);
        if (request == null) return NotFound();

        if (!Enum.IsDefined(dto.NewState))
        {
            return BadRequest("Invalid request state.");
        }

        if (!RequestStateTransitionPolicy.IsAllowed(request.State, dto.NewState))
        {
            return BadRequest($"Cannot transition from {request.State} to {dto.NewState}.");
        }

        if (RequestStateTransitionPolicy.RequiresReason(dto.NewState) && string.IsNullOrWhiteSpace(dto.Reason))
        {
            return BadRequest($"A reason is required when transitioning to {dto.NewState}.");
        }

        if (dto.NewState == RequestState.Assigned && string.IsNullOrWhiteSpace(request.AssigneeId))
        {
            return BadRequest("An assignee is required when transitioning to Assigned.");
        }

        var oldState = request.State;
        request.State = dto.NewState;
        var actor = GetActor();

        _context.StatusHistories.Add(new StatusHistory
        {
            RequestId = request.Id,
            PreviousState = oldState,
            NewState = request.State,
            ChangedAt = DateTime.UtcNow,
            ChangedBy = actor,
            Reason = dto.Reason
        });

        await _context.SaveChangesAsync();

        // Notify observers (Design Problem 1: Observer)
        await _notifier.NotifyStateChangedAsync(request, oldState, request.State);

        return Ok(request);
    }

    private string GetActor() => User.Identity?.Name ?? "Anonymous";
}

public class CreateRequestDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int CategoryId { get; set; }
}

public class UpdateStateDto
{
    public RequestState NewState { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class AssignRequestDto
{
    public string AssigneeId { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}

public class StaffRequestListItemDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public RequestState State { get; set; }
    public DateTime CreatedAt { get; set; }
    public string RequesterId { get; set; } = string.Empty;
    public string? AssigneeId { get; set; }
}

public class PagedResultDto<T>
{
    public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
}
