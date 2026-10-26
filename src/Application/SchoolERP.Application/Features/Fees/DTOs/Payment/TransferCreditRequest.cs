namespace SchoolERP.Application.Features.Fees.DTOs.Payment;

public sealed class TransferCreditRequest
{
    public long FromStudentId { get; set; }
    public long ToStudentId { get; set; }
    public decimal Amount { get; set; }
    public string Reason { get; set; } = string.Empty;
}
