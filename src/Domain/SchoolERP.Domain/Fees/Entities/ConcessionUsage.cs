using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Fees.Entities;

/// <summary>Immutable record of a concession consumed by an invoice. Written during invoice generation.</summary>
public sealed class ConcessionUsage : IdEntity
{
    public long ConcessionAllocationId { get; set; }
    public long InvoiceId { get; set; }
    public decimal AppliedAmount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
