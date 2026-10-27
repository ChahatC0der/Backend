namespace SchoolERP.Application.Features.Fees.DTOs.Portal;

public sealed class OptionalFeeResponse
{
    public long FeeHeadId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal? SuggestedAmount { get; set; }
    public bool AlreadyOptedIn { get; set; }
}
