using FeeheadEntity = SchoolERP.Domain.Fees.Entities.FeeHead;

namespace SchoolERP.Application.Features.Fees.DTOs.FeeHead;

public sealed class FeeHeadResponse
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsRefundable { get; set; }
    public bool IsMandatory { get; set; }
    public bool IsOptional { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    /// <summary>Status returns the enum name in PascalCase (e.g. "Active", "Inactive").</summary>
    public static FeeHeadResponse FromEntity(FeeheadEntity e) => new()
    {
        Id = e.Id,
        Name = e.Name,
        Code = e.Code,
        Description = e.Description,
        IsRefundable = e.IsRefundable,
        IsMandatory = e.IsMandatory,
        IsOptional = e.IsOptional,
        Status = e.Status.ToString(),
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt,
    };
}
