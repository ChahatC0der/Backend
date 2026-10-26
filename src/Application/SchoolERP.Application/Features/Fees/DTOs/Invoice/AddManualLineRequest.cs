namespace SchoolERP.Application.Features.Fees.DTOs.Invoice;

public sealed class AddManualLineRequest
{
    public long FeeHeadId { get; set; }
    public decimal Amount { get; set; }
}
