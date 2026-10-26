namespace SchoolERP.Application.Features.Fees.DTOs.Concession;

public sealed class ConcessionAllocationResponse
{
    public long Id { get; set; }
    public long ConcessionId { get; set; }
    public string ConcessionName { get; set; } = string.Empty;
    public long StudentId { get; set; }
    public long AcademicYearId { get; set; }
    public string Status { get; set; } = string.Empty;
    public long? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
