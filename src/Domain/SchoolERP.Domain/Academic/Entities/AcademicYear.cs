using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Academic.Entities;

public class AcademicYear : BranchEntity,IMustHaveBranch
{
    public string Name { get; set; } = string.Empty;       // e.g., "2025-26"
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public bool IsCurrent { get; set; }
    public string Status { get; set; } = "upcoming";       // upcoming, active, closed
}