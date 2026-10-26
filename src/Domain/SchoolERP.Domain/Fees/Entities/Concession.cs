using SchoolERP.Domain.Common;
using SchoolERP.Domain.Fees.Enums;

namespace SchoolERP.Domain.Fees.Entities;

/// <summary>Branch-scoped concession definition. Physical dbo.Concessions has DeletedAt only.</summary>
public sealed class Concession : BaseBranchEntity
{
    public string Name { get; set; } = string.Empty;
    public ConcessionType Type { get; set; }
    public decimal Value { get; set; }
    public long? FeeHeadId { get; set; }
    public bool IsAutoApply { get; set; }
    public bool ApprovalRequired { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
