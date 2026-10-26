namespace SchoolERP.Application.Features.Fees.DTOs.Invoice;

public sealed class StudentFeeCounterResponse
{
    public long StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string EnrollmentId { get; set; } = string.Empty;
    public long ClassId { get; set; }
    public long? SectionId { get; set; }
    public long? FamilyId { get; set; }

    public long AcademicYearId { get; set; }
    public string AcademicYearName { get; set; } = string.Empty;

    public long? FeeStructureId { get; set; }
    public string? StructureName { get; set; }
    public string ResolvedFrom { get; set; } = "none";

    public decimal WalletBalance { get; set; }
    public decimal TotalOutstanding { get; set; }
    public List<InvoiceResponse> PendingInvoices { get; set; } = new();
}
