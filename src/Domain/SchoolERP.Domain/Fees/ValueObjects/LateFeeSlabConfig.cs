namespace SchoolERP.Domain.Fees.ValueObjects;

/// <summary>
/// Shape of the JSON stored in FeeStructureLines.LateFeeSlabConfig
/// and InvoiceLines.LateFeeSlabConfigSnapshot for Graduated late-fee type.
/// Example: [{ "fromDay": 1, "toDay": 10, "amount": 50 },
///           { "fromDay": 11, "toDay": 20, "amount": 100 }]
/// </summary>
public sealed class LateFeeSlabConfig
{
    public int FromDay { get; set; }
    public int ToDay { get; set; }
    public decimal Amount { get; set; }
}
