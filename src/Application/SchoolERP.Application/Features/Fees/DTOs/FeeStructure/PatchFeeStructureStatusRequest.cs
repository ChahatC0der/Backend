namespace SchoolERP.Application.Features.Fees.DTOs.FeeStructure;

public sealed class PatchFeeStructureStatusRequest
{
    public string Status { get; set; } = string.Empty;   // draft | active | archived
}
