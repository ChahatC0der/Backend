namespace SchoolERP.Application.Features.Fees.DTOs.FeeStructure;

public sealed class CloneFeeStructureRequest
{
    public long TargetAcademicYearId { get; set; }
    public decimal? PercentageAdjustment { get; set; }   // e.g. 10 = +10%
    public decimal? FlatAdjustment { get; set; }         // added to each line
    public string? NewName { get; set; }
}
