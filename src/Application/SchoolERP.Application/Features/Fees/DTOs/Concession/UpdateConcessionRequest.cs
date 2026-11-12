using SchoolERP.Domain.Fees.Enums;

namespace SchoolERP.Application.Features.Fees.DTOs.Concession;

public sealed class UpdateConcessionRequest
{
    public string Name { get; set; } = string.Empty;
    public ConcessionCategory Category { get; set; }
    public ConcessionCalculationType CalculationType { get; set; }
    public decimal Value { get; set; }
    public long? FeeHeadId { get; set; }
    public bool IsAutoApply { get; set; }
    public bool ApprovalRequired { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}