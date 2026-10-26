namespace SchoolERP.Application.Features.Fees.DTOs.FeeStructure;

public sealed class StudentAssignedStructureResponse
{
    public long StudentId { get; set; }
    public long AcademicYearId { get; set; }
    public long? FeeStructureId { get; set; }
    public string? StructureName { get; set; }
    public string ResolvedFrom { get; set; } = string.Empty;   // "override" | "section" | "class" | "classgroup" | "none"
    public decimal? OverrideAmount { get; set; }
    public List<FeeStructureLineResponse> Lines { get; set; } = new();
}
