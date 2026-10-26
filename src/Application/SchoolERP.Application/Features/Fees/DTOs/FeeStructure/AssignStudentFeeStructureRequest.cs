namespace SchoolERP.Application.Features.Fees.DTOs.FeeStructure;

public sealed class AssignStudentFeeStructureRequest
{
    public long StudentId { get; set; }
    public long FeeStructureId { get; set; }
    public long AcademicYearId { get; set; }
    public decimal? OverrideAmount { get; set; }
}
