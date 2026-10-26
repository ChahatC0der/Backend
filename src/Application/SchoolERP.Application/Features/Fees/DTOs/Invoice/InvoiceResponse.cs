namespace SchoolERP.Application.Features.Fees.DTOs.Invoice;

public sealed class InvoiceResponse
{
    public long Id { get; set; }
    public long StudentId { get; set; }
    public long AcademicYearId { get; set; }
    public long ClassId { get; set; }
    public long? SectionId { get; set; }
    public long? FeeStructureId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string PeriodLabel { get; set; } = string.Empty;
    public DateTime GenerationDate { get; set; }
    public DateTime DueDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal ConcessionAmount { get; set; }
    public decimal LateFeeAmount { get; set; }
    public decimal NetPayable { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal CreditAppliedAmount { get; set; }
    public decimal Outstanding { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<InvoiceLineResponse> Lines { get; set; } = new();
}
