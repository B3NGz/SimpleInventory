namespace SimpleInventorySystem.Model.Entities;

public class AuditLog
{
    public long AuditLogId { get; set; }

    public int? UserId { get; set; }

    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public long EntityId { get; set; }

    public string? OldValues { get; set; }
    public string? NewValues { get; set; }

    public string? Remarks { get; set; }

    public DateTime CreatedAt { get; set; }
}