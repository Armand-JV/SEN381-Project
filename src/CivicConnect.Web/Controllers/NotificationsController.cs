using System;
using System.Security.Claims;
using System.Threading.Tasks;
using CivicConnect.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CivicConnect.Domain.Entities;

namespace CivicConnect.Web.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize(Roles = "REQUESTER")]
public class NotificationsController : ControllerBase
{
    private readonly CivicConnectDbContext _context;

    public NotificationsController(CivicConnectDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetNotifications(
        [FromQuery] bool unreadOnly = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100)
        {
            return BadRequest("Page must be at least 1 and pageSize must be between 1 and 100.");
        }

        var recipientId = GetActor();
        var query = _context.Notifications
            .AsNoTracking()
            .Where(notification => notification.RecipientId == recipientId);
        if (unreadOnly)
        {
            query = query.Where(notification => notification.ReadAt == null);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(notification => notification.CreatedAt)
            .ThenByDescending(notification => notification.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(notification => new NotificationDto
            {
                Id = notification.Id,
                RequestId = notification.RequestId,
                Message = notification.Message,
                CreatedAt = notification.CreatedAt,
                ReadAt = notification.ReadAt
            })
            .ToListAsync();

        return Ok(new PagedResultDto<NotificationDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        });
    }

    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkRead(int id)
    {
        var recipientId = GetActor();
        var notification = await _context.Notifications
            .SingleOrDefaultAsync(item => item.Id == id && item.RecipientId == recipientId);
        if (notification == null) return NotFound();

        if (notification.ReadAt == null)
        {
            notification.ReadAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        return NoContent();
    }

    private string GetActor() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub")
        ?? User.Identity?.Name
        ?? throw new InvalidOperationException("The authenticated principal has no actor identifier.");
}

public class NotificationDto
{
    public int Id { get; set; }
    public int RequestId { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ReadAt { get; set; }
}
