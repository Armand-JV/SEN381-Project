using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using AssignedTransitionPolicy = CivicConnect.Application.Policies.RequestStateTransitionPolicy;
using CivicConnect.Application.Patterns.Observer;
using CivicConnect.Application.Patterns.Strategy;
using CivicConnect.Application.Services;
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
    private readonly IAccessControlService _accessControl;

    public RequestsController(
        CivicConnectDbContext context,
        CategoryValidationContext validationContext,
        RequestStateNotifier notifier,
        IAccessControlService accessControl)
    {
        _context = context;
        _validationContext = validationContext;
        _notifier = notifier;
        _accessControl = accessControl;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateRequest([FromBody] CreateRequestDto dto)
    {
        var category = await _context.Categories.FindAsync(dto.CategoryId);
        if (category == null) return BadRequest("Invalid category.");

        var actor = GetActor();

        var request = new Request
        {
            Title = dto.Title,
            Description = dto.Description,
            CategoryId = dto.CategoryId,
            CreatedAt = DateTime.UtcNow,
            RequesterId = actor,
            State = RequestState.Open
        };

        if (!_validationContext.Validate(request))
        {
            return BadRequest("Request validation failed for the selected category.");
        }


        // Initial state log
        request.History.Add(new StatusHistory
        {
            PreviousState = RequestState.Open,
            NewState = RequestState.Open,
            ChangedAt = DateTime.UtcNow,
            ChangedBy = actor,
            Reason = "Initial submission"
        });

        _context.Requests.Add(request);

        await _context.SaveChangesAsync();

        var created = new CreatedRequestDto
        {
            Id = request.Id,
            Title = request.Title,
            Description = request.Description,
            CategoryId = request.CategoryId,
            RequesterId = request.RequesterId,
            State = request.State,
            CreatedAt = request.CreatedAt
        };

        return CreatedAtAction(nameof(GetRequest), new { id = request.Id }, created);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "REQUESTER,STAFF,MANAGER")]
    public async Task<IActionResult> GetRequest(int id)
    {
        var request = await _context.Requests
            .AsNoTracking()
            .Include(r => r.Category)
            .Include(r => r.Comments)
            .FirstOrDefaultAsync(r => r.Id == id);
        if (request == null) return NotFound();

        if (!_accessControl.CanAccess(User, request)) return Forbid();

        if (User.IsInRole("REQUESTER"))
        {
            request.Comments = request.Comments.Where(comment => !comment.IsInternal).ToList();
        }

        return Ok(request);
    }

    [HttpPost("{id}/comments")]
    [Authorize(Roles = "REQUESTER,STAFF,MANAGER")]
    public async Task<IActionResult> AddComment(int id, [FromBody] AddCommentDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Text))
        {
            return BadRequest("Comment text is required.");
        }

        var request = await _context.Requests.FindAsync(id);
        if (request == null) return NotFound();

        var actor = GetActor();
        if (User.IsInRole("REQUESTER"))
        {
            if (request.RequesterId != actor) return Forbid();
            if (dto.IsInternal) return Forbid();
        }

        var comment = new Comment
        {
            RequestId = request.Id,
            Text = dto.Text.Trim(),
            CreatedAt = DateTime.UtcNow,
            CreatedBy = actor,
            IsInternal = dto.IsInternal
        };
        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetRequest), new { id = request.Id }, comment);
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

        if (!AssignedTransitionPolicy.IsAllowed(request.State, RequestState.Assigned))
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

        if (!RequestStateTransitionPolicy.TryValidate(request, dto.NewState, User, dto.Reason, out var error))
        {
            return BadRequest(error);
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
            Reason = string.IsNullOrWhiteSpace(dto.Reason) ? null : dto.Reason.Trim()
        });

        // NFR-020 (Issue #35): append-only audit entry in the same transaction.
        _context.AuditLogs.Add(new AuditLog
        {
            Actor = actor,
            Action = "StateChanged",
            EntityType = nameof(Request),
            EntityId = request.Id,
            BeforeSummary = oldState.ToString(),
            AfterSummary = request.State.ToString(),
            Timestamp = DateTime.UtcNow,
            CorrelationId = Guid.NewGuid().ToString()
        });

        await _context.SaveChangesAsync();

        // Notify observers (Design Problem 1: Observer)
        await _notifier.NotifyStateChangedAsync(request, oldState, request.State);

        return Ok(request);
    }

    private string GetActor() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub")
        ?? User.Identity?.Name
        ?? throw new InvalidOperationException("The authenticated principal has no actor identifier.");
}

public class CreateRequestDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int CategoryId { get; set; }
}

public class CreatedRequestDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string RequesterId { get; set; } = string.Empty;
    public RequestState State { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UpdateStateDto
{
    public RequestState NewState { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class AddCommentDto
{
    public string Text { get; set; } = string.Empty;
    public bool IsInternal { get; set; }
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
