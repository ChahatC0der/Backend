using ConcessionEntity = SchoolERP.Domain.Fees.Entities.Concession;

namespace SchoolERP.Application.Features.Fees.DTOs.Concession;

public sealed class ConcessionResponse
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public long? FeeHeadId { get; set; }
    public string? FeeHeadName { get; set; }
    public bool IsAutoApply { get; set; }
    public bool ApprovalRequired { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public static ConcessionResponse FromEntity(ConcessionEntity e, string? feeHeadName = null) => new()
    {
        Id = e.Id,
        Name = e.Name,
        Type = e.Type.ToString(),
        Value = e.Value,
        FeeHeadId = e.FeeHeadId,
        FeeHeadName = feeHeadName,
        IsAutoApply = e.IsAutoApply,
        ApprovalRequired = e.ApprovalRequired,
        StartDate = e.StartDate,
        EndDate = e.EndDate,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt,
    };
}
