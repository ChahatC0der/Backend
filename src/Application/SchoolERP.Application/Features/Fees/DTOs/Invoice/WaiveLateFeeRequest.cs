namespace SchoolERP.Application.Features.Fees.DTOs.Invoice;

public sealed class WaiveLateFeeRequest
{
    public decimal? WaiveAmount { get; set; }   // null = waive all
    public string WaiverReason { get; set; } = string.Empty;
}
