using SchoolERP.Domain.Common;
using SchoolERP.Domain.Fees.Enums;

namespace SchoolERP.Domain.Fees.Entities;

/// <summary>
/// Tenant + branch scoped fee head.
/// Physical table: dbo.FeeHeads (no IsDeleted column — soft delete via DeletedAt only).
/// </summary>
public sealed class FeeHead : BaseBranchEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsRefundable { get; set; }
    public bool IsMandatory { get; set; } = true;
    public bool IsOptional { get; set; }
    public FeeHeadStatus Status { get; set; } = FeeHeadStatus.Active;
}
