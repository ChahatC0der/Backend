using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Fees.Entities;

/// <summary>
/// Active student-specific override of a fee structure.
/// Unique filtered index (StudentId, AcademicYearId) WHERE IsActive = 1.
/// </summary>
public sealed class StudentFeeAssignment : IdEntity, IAuditableTimestamps
{
    public long StudentId { get; set; }
    public long FeeStructureId { get; set; }
    public long AcademicYearId { get; set; }
    public decimal? OverrideAmount { get; set; }
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
