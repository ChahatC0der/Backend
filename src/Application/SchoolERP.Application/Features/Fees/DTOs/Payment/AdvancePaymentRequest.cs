namespace SchoolERP.Application.Features.Fees.DTOs.Payment;

public sealed class AdvancePaymentRequest
{
    public long StudentId { get; set; }
    public long? AcademicYearId { get; set; }
    public DateTime PaymentDate { get; set; }
    public decimal Amount { get; set; }
    public string Mode { get; set; } = "cash";
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
}
