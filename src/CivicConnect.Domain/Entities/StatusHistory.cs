using System;
using CivicConnect.Domain.Enums;

namespace CivicConnect.Domain.Entities;

public class StatusHistory
{
    public int Id { get; set; }
    public int RequestId { get; set; }
    public RequestState PreviousState { get; set; }
    public RequestState NewState { get; set; }
    public DateTime ChangedAt { get; set; }
    public string ChangedBy { get; set; } = string.Empty;
    public string? Reason { get; set; }
}
