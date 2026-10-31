using SchoolERP.Domain.Common;
using SchoolERP.Domain.Fees.Enums;

namespace SchoolERP.Domain.Fees.Entities;

/// <summary>
/// Child of FeeStructure. Physical dbo.FeeStructureLines has DeletedAt only (no IsDeleted).
/// LateFeeSlabConfig holds JSON only when LateFeeType = Graduated.
/// </summary>
public sealed class FeeStructureLine : BaseEntity
{
    public long FeeStructureId { get; set; }
    public long FeeHeadId { get; set; }
    public decimal Amount { get; set; }
    public FeeFrequency Frequency { get; set; } = FeeFrequency.Monthly;
    public short? DueDay { get; set; }
    public bool LateFeeApplicable { get; set; }
    public LateFeeType? LateFeeType { get; set; }
    public decimal? LateFeeValue { get; set; }
    public short? LateFeeGraceDays { get; set; }
    public decimal? LateFeeCapPercent { get; set; }
    public string? LateFeeSlabConfig { get; set; }
}
