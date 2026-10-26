namespace SchoolERP.Application.Features.Fees.DTOs.FeeStructure;

public sealed class UpdateFeeStructureRequest
{
    public string Name { get; set; } = string.Empty;
    public DateTime ApplicableFrom { get; set; }
    public DateTime ApplicableTo { get; set; }
    public string ProrationMethod { get; set; } = "full_month";
    public string PaymentAllocationPriority { get; set; } = "manual_only";
    public List<FeeStructureLineDto> Lines { get; set; } = new();
}
