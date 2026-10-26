namespace SchoolERP.Application.Features.Fees.DTOs.Concession;

public sealed class CreateConcessionRequest
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "percentage";
    public decimal Value { get; set; }
    public long? FeeHeadId { get; set; }
    public bool IsAutoApply { get; set; }
    public bool ApprovalRequired { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
