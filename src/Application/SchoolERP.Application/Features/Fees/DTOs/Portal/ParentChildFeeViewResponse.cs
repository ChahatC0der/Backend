namespace SchoolERP.Application.Features.Fees.DTOs.Portal;

/// <summary>
/// Parent-facing view of a child's fee data.
/// Uses school-facing terminology: "Fee Bill" not Invoice, "Fee Receipt" not Payment.
/// </summary>
public sealed class ParentChildFeeViewResponse
{
    public long ChildId { get; set; }
    public string ChildName { get; set; } = string.Empty;
    public string EnrollmentId { get; set; } = string.Empty;
    public long AcademicYearId { get; set; }
    public string AcademicYearName { get; set; } = string.Empty;

    public decimal AdvanceWalletBalance { get; set; }
    public decimal TotalOutstanding { get; set; }

    public List<ParentFeeBillDto> FeeBills { get; set; } = new();
    public List<ParentFeeReceiptDto> FeeReceipts { get; set; } = new();
}

public sealed class ParentFeeBillDto
{
    public long BillId { get; set; }
    public string BillNumber { get; set; } = string.Empty;
    public string PeriodLabel { get; set; } = string.Empty;
    public DateTime BillDate { get; set; }
    public DateTime DueDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal ConcessionAmount { get; set; }
    public decimal LateFeeAmount { get; set; }
    public decimal NetPayable { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal WalletApplied { get; set; }
    public decimal Outstanding { get; set; }
    public string Status { get; set; } = string.Empty;
}

public sealed class ParentFeeReceiptDto
{
    public long ReceiptId { get; set; }
    public string ReceiptNumber { get; set; } = string.Empty;
    public DateTime ReceiptDate { get; set; }
    public decimal Amount { get; set; }
    public string Mode { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
