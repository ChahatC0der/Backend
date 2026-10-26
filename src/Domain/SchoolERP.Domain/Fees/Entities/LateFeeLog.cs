using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Fees.Entities;

/// <summary>
/// One row per invoice per day when late fee is calculated.
/// Physical table: dbo.LateFeeLogs.
/// Idempotency: unique index on (InvoiceId, CalculationDateOnly) enforced by EF config.
/// Writes are DEFERRED — entity and config only in Phase 0.
/// </summary>
public sealed class LateFeeLog : IdEntity
{
    public long InvoiceId { get; set; }
    public DateTime CalculatedDate { get; set; }
    public decimal LateFeeAmount { get; set; }
    public decimal? WaivedAmount { get; set; }
    public long? WaivedBy { get; set; }
    public string? WaiverReason { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
