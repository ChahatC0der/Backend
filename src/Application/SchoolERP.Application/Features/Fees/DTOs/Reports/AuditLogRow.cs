namespace SchoolERP.Application.Features.Fees.DTOs.Reports;

public sealed class AuditLogRow
{
    public long Id { get; set; }
    public Guid? TenantId { get; set; }
    public string TableName { get; set; } = string.Empty;
    public long RecordId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public long PerformedBy { get; set; }
    public DateTime CreatedAt { get; set; }
}
