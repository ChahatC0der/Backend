namespace SchoolERP.Application.Features.Fees.DTOs.FeeStructure;

public sealed class FeeStructureResponse
{
    public long Id { get; set; }
    public long AcademicYearId { get; set; }
    public long? ClassGroupId { get; set; }
    public long? ClassId { get; set; }
    public long? SectionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime ApplicableFrom { get; set; }
    public DateTime ApplicableTo { get; set; }
    public string ProrationMethod { get; set; } = string.Empty;
    public string PaymentAllocationPriority { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<FeeStructureLineResponse> Lines { get; set; } = new();
}
