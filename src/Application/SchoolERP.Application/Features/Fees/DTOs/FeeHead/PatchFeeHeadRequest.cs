namespace SchoolERP.Application.Features.Fees.DTOs.FeeHead;

public sealed class PatchFeeHeadRequest
{
    public string? Name { get; set; }
    public string? Code { get; set; }
    public string? Description { get; set; }
    public bool? IsRefundable { get; set; }
    public bool? IsMandatory { get; set; }
    public bool? IsOptional { get; set; }
}
