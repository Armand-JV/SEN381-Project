using System;
using System.Text.Json.Serialization;

namespace CivicConnect.Domain.Entities;

public class AuditEntry
{
    public int Id { get; set; }
    public int RequestId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string ActorId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string Details { get; set; } = string.Empty;

    [JsonIgnore]
    public Request? Request { get; set; }
}
