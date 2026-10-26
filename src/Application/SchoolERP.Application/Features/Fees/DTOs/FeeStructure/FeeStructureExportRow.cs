namespace SchoolERP.Application.Features.Fees.DTOs.FeeStructure;

public sealed class FeeStructureExportRow
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public long AcademicYearId { get; set; }
    public long? ClassGroupId { get; set; }
    public long? ClassId { get; set; }
    public long? SectionId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string ProrationMethod { get; set; } = string.Empty;
    public DateTime ApplicableFrom { get; set; }
    public DateTime ApplicableTo { get; set; }
    public int LineCount { get; set; }
}
