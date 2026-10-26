using SchoolERP.Domain.Common;
using SchoolERP.Domain.Fees.Enums;

namespace SchoolERP.Domain.Fees.Entities;

/// <summary>Assignment of a Concession to a specific student + academic year.</summary>
public sealed class ConcessionAllocation : IdEntity
{
    public long ConcessionId { get; set; }
    public long StudentId { get; set; }
    public long AcademicYearId { get; set; }
    public ConcessionAllocationStatus Status { get; set; } = ConcessionAllocationStatus.Pending;
    public long? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
