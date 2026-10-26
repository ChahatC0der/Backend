namespace SchoolERP.Application.Features.Fees.DTOs.Payment;

public sealed class CollectPaymentRequest
{
    public long StudentId { get; set; }
    public long AcademicYearId { get; set; }
    public DateTime PaymentDate { get; set; }
    public decimal TotalAmountReceived { get; set; }
    public string Mode { get; set; } = "cash";
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
    public List<InvoiceAllocationItem> Allocations { get; set; } = new();
}
