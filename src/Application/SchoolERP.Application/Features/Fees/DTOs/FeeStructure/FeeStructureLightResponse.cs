namespace SchoolERP.Application.Features.Fees.DTOs.FeeStructure;

public sealed class FeeStructureLightResponse
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public long AcademicYearId { get; set; }
    public string Status { get; set; } = string.Empty;
}
