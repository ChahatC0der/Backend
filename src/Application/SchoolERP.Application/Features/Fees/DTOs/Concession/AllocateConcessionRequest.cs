namespace SchoolERP.Application.Features.Fees.DTOs.Concession;

public sealed class AllocateConcessionRequest
{
    public long ConcessionId { get; set; }
    public long AcademicYearId { get; set; }
    public long? StudentId { get; set; }
    public long? ClassId { get; set; }
    public long? SectionId { get; set; }
}
