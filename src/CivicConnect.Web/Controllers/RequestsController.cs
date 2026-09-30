using System;
using System.Threading.Tasks;
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
            RequesterId = "user123", // Simulated logged in user
            State = RequestState.Open
        };

        if (!_validationContext.Validate(request))
        {
            return BadRequest("Request validation failed for the selected category.");
        }

        _context.Requests.Add(request);
        
        // Initial state log
        _context.StatusHistories.Add(new StatusHistory
        {
            RequestId = request.Id, // Link by ID since EF Core might require proper linking
            PreviousState = RequestState.Open,
            NewState = RequestState.Open,
            ChangedAt = DateTime.UtcNow,
            ChangedBy = "user123",
            Reason = "Initial submission"
        });

        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetRequest), new { id = request.Id }, request);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetRequest(int id)
    {
        var request = await _context.Requests.Include(r => r.Category).FirstOrDefaultAsync(r => r.Id == id);
        if (request == null) return NotFound();
        return Ok(request);
    }

    [HttpPut("{id}/state")]
    public async Task<IActionResult> UpdateState(int id, [FromBody] UpdateStateDto dto)
    {
        var request = await _context.Requests.FindAsync(id);
        if (request == null) return NotFound();

        var oldState = request.State;
        request.State = dto.NewState;

        _context.StatusHistories.Add(new StatusHistory
        {
            RequestId = request.Id,
            PreviousState = oldState,
            NewState = request.State,
            ChangedAt = DateTime.UtcNow,
            ChangedBy = "admin", // Simulated admin
            Reason = dto.Reason
        });

        await _context.SaveChangesAsync();

        // Notify observers (Design Problem 1: Observer)
        await _notifier.NotifyStateChangedAsync(request, oldState, request.State);

        return Ok(request);
    }
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
