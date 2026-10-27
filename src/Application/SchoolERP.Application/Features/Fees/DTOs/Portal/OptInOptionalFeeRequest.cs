namespace SchoolERP.Application.Features.Fees.DTOs.Portal;

public sealed class OptInOptionalFeeRequest
{
    public long StudentId { get; set; }
    public long AcademicYearId { get; set; }
    public long FeeHeadId { get; set; }
    public decimal Amount { get; set; }
    public DateTime? DueDate { get; set; }
}
