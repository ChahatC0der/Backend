namespace SchoolERP.Application.Features.Fees.DTOs.Reports;

public sealed class CollectionReportRow
{
    public string Mode { get; set; } = string.Empty;
    public long? CollectedBy { get; set; }
    public int PaymentCount { get; set; }
    public decimal TotalCollected { get; set; }
}
