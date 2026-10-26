using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Fees.Entities;

/// <summary>
/// Composite PK (BranchId, AcademicYearCode, SequenceType). No surrogate Id.
/// SequenceType is "INVOICE" or "RECEIPT" (uppercase, matching physical DB).
/// </summary>
public sealed class InvoiceSequence : IMustHaveBranch
{
    public Guid BranchId { get; set; }
    public string AcademicYearCode { get; set; } = string.Empty;
    public string SequenceType { get; set; } = string.Empty;
    public long CurrentSequence { get; set; }
}
