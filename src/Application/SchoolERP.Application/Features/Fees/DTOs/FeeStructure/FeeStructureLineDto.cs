namespace SchoolERP.Application.Features.Fees.DTOs.FeeStructure;

public sealed class FeeStructureLineDto
{
    public long? Id { get; set; }                 // null = new line
    public long FeeHeadId { get; set; }
    public decimal Amount { get; set; }
    public string Frequency { get; set; } = "monthly";   // one_time | monthly | quarterly | half_yearly | annual
    public int? DueDay { get; set; }
    public bool LateFeeApplicable { get; set; }
    public string? LateFeeType { get; set; }      // stagnant_fixed | graduated | monthly
    public decimal? LateFeeValue { get; set; }
    public int? LateFeeGraceDays { get; set; }
    public decimal? LateFeeCapPercent { get; set; }
    public string? LateFeeSlabConfig { get; set; }
}
