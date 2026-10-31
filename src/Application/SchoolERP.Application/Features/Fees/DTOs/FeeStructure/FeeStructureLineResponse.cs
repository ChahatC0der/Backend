namespace SchoolERP.Application.Features.Fees.DTOs.FeeStructure;

public sealed class FeeStructureLineResponse
{
    public long Id { get; set; }
    public long FeeHeadId { get; set; }
    public string FeeHeadName { get; set; } = string.Empty;
    public string FeeHeadCode { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Frequency { get; set; } = string.Empty;
    public short? DueDay { get; set; }
    public bool LateFeeApplicable { get; set; }
    public string? LateFeeType { get; set; }
    public decimal? LateFeeValue { get; set; }
    public short? LateFeeGraceDays { get; set; }
    public decimal? LateFeeCapPercent { get; set; }
    public string? LateFeeSlabConfig { get; set; }
}
