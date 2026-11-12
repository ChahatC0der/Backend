namespace SchoolERP.Application.Features.Fees.DTOs.Concession;

public sealed class ConcessionExportRow
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string CalculationType { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public string? FeeHead { get; set; }
    public string IsAutoApply { get; set; } = string.Empty;
    public string ApprovalRequired { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}