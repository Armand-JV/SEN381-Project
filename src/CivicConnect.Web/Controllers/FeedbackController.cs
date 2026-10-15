using System.Security.Claims;
using CivicConnect.Domain.Entities;
using CivicConnect.Domain.Enums;
using CivicConnect.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CivicConnect.Web.Controllers;

[ApiController]
[Route("api/requests/{requestId:int}/feedback")]
[Authorize(Roles = "REQUESTER")]
public sealed class FeedbackController : ControllerBase
{
    private readonly CivicConnectDbContext _context;

    public FeedbackController(CivicConnectDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<ActionResult<FeedbackDto>> Submit(int requestId, [FromBody] SubmitFeedbackDto dto)
    {
        var errors = new Dictionary<string, string[]>();
        if (dto.Rating is < 1 or > 5)
        {
            errors["rating"] = new[] { "Rating must be between 1 and 5." };
        }

        if (dto.Comment?.Trim().Length > 2000)
        {
            errors["comment"] = new[] { "Comment must be 2000 characters or fewer." };
        }

        if (errors.Count > 0)
        {
            return ValidationProblem(new ValidationProblemDetails(errors));
        }

        var request = await _context.Requests
            .Include(item => item.Feedback)
            .FirstOrDefaultAsync(item => item.Id == requestId);
        if (request == null) return NotFound();

        var actor = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? User.Identity?.Name;
        if (actor is null || request.RequesterId != actor) return Forbid();
        if (request.State is not (RequestState.Resolved or RequestState.Closed))
        {
            return Conflict("Feedback is available only for resolved or closed requests.");
        }

        if (request.Feedback is not null)
        {
            return Conflict("Feedback has already been submitted for this request.");
        }

        var feedback = new Feedback
        {
            RequestId = request.Id,
            Rating = dto.Rating,
            Comment = dto.Comment?.Trim() ?? string.Empty,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = actor
        };
        _context.Feedback.Add(feedback);
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return Conflict("Feedback has already been submitted for this request.");
        }

        return CreatedAtAction(nameof(GetFeedback), new { requestId }, ToDto(feedback));
    }

    [HttpGet]
    public async Task<ActionResult<FeedbackDto>> GetFeedback(int requestId)
    {
        var request = await _context.Requests
            .AsNoTracking()
            .Include(item => item.Feedback)
            .FirstOrDefaultAsync(item => item.Id == requestId);
        if (request == null) return NotFound();

        var actor = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? User.Identity?.Name;
        if (actor is null || request.RequesterId != actor) return Forbid();
        return request.Feedback is null ? NotFound() : Ok(ToDto(request.Feedback));
    }

    private static FeedbackDto ToDto(Feedback feedback) => new()
    {
        Id = feedback.Id,
        RequestId = feedback.RequestId,
        Rating = feedback.Rating,
        Comment = feedback.Comment,
        CreatedAt = feedback.CreatedAt,
        CreatedBy = feedback.CreatedBy
    };
}

public sealed class SubmitFeedbackDto
{
    public int Rating { get; set; }
    public string? Comment { get; set; }
}

public sealed class FeedbackDto
{
    public int Id { get; set; }
    public int RequestId { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}
