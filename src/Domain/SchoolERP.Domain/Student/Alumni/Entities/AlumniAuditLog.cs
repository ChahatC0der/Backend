using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Student.Alumni.Entities;

public class AlumniAuditLog : BaseEntity
{
    public Guid TenantId { get; set; }
    public long AlumniId { get; set; }
    public long PerformedBy { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }

    public Alumni Alumni { get; set; } = null!;
}
