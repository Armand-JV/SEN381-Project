using System;

namespace CivicConnect.Domain.Entities;

public class Feedback
{
    public int Id { get; set; }
    public int RequestId { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;

    public Request? Request { get; set; }
}
