using System.Text.Json;
using SimpleInventorySystem.Data;
using SimpleInventorySystem.Model.Entities;

namespace SimpleInventorySystem.Service;

public class AuditLogService
{
    private readonly ApplicationDbContext _context;

    public AuditLogService(ApplicationDbContext context)
    {
        _context = context;
    }

    public void AddLog(
        int? userId,
        string action,
        string entityType,
        long entityId,
        object? oldValues = null,
        object? newValues = null,
        string? remarks = null)
    {
        var auditLog = new AuditLog
        {
            UserId = userId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,

            OldValues = oldValues == null
                ? null
                : JsonSerializer.Serialize(oldValues),

            NewValues = newValues == null
                ? null
                : JsonSerializer.Serialize(newValues),

            Remarks = remarks,
            CreatedAt = DateTime.UtcNow
        };

        _context.AuditLogs.Add(auditLog);
    }
}