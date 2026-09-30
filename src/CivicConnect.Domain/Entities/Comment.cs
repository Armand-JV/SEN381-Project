using System;

namespace CivicConnect.Domain.Entities;

public class Comment
{
    public int Id { get; set; }
    public int RequestId { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public bool IsInternal { get; set; }
}
