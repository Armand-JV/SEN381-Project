using System;
using System.Collections.Generic;
using CivicConnect.Domain.Enums;

namespace CivicConnect.Domain.Entities;

public class Request
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public Category? Category { get; set; }
    public RequestState State { get; set; } = RequestState.Open;
    public DateTime CreatedAt { get; set; }
    public string RequesterId { get; set; } = string.Empty;
    public string? AssigneeId { get; set; }
    
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<StatusHistory> History { get; set; } = new List<StatusHistory>();
}
