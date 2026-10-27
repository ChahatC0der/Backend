namespace SchoolERP.Application.Features.Fees.DTOs.Reports;

public sealed class StudentLedgerRow
{
    public DateTime EntryDate { get; set; }
    public string EntryType { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public decimal RunningBalance { get; set; }
    public string Status { get; set; } = string.Empty;
}
