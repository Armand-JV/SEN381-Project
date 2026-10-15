using System;

namespace CivicConnect.Domain.Entities;

// NFR-020 (Issue #35): append-only audit record for consequential actions.
// Separate from requester-facing StatusHistory. No update/delete path is exposed.
public class AuditLog
{
    public int Id { get; set; }
    public string Actor { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public string? BeforeSummary { get; set; }
    public string? AfterSummary { get; set; }
    public DateTime Timestamp { get; set; }
    public string? CorrelationId { get; set; }
}
