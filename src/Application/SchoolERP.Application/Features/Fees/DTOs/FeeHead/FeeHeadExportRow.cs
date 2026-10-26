namespace SchoolERP.Application.Features.Fees.DTOs.FeeHead;

/// <summary>Flat DTO for reflection-based Excel export — public properties only.</summary>
public sealed class FeeHeadExportRow
{
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public string IsMandatory { get; set; } = string.Empty;
    public string IsOptional { get; set; } = string.Empty;
    public string IsRefundable { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
