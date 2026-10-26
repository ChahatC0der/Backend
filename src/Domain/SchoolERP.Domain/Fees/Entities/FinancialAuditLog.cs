using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Fees.Entities;

/// <summary>
/// Financial audit trail. Inherits BaseAuditLog (Id, TenantId?, PerformedBy, CreatedAt).
/// Physical table: dbo.FinancialAuditLogs.
/// Writes are DEFERRED — entity and config only in Phase 0.
/// </summary>
public sealed class FinancialAuditLog : BaseAuditLog
{
    public string TableName { get; set; } = string.Empty;
    public long RecordId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
}
