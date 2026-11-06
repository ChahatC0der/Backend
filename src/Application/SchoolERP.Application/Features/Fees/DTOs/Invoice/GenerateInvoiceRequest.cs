namespace SchoolERP.Application.Features.Fees.DTOs.Invoice;

public sealed class GenerateInvoiceRequest
{
    public long StudentId { get; set; }
    public long AcademicYearId { get; set; }
    /// <summary>
    /// Identifies the billing period within the Academic Year.
    /// Example: 1 = first period, 2 = second period.
    /// The actual dates will be calculated by BillingPeriodCalculator.
    /// </summary>
    public int PeriodSequenceNumber { get; set; }
    public DateTime? DueDate { get; set; }
}
