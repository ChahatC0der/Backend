namespace SchoolERP.Application.Features.Fees.DTOs.FeeHead;

public sealed class CreateFeeHeadRequest
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsRefundable { get; set; }
    public bool IsMandatory { get; set; } = true;
    public bool IsOptional { get; set; }
}
