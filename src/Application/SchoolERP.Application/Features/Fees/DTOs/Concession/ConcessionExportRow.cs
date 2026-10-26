namespace SchoolERP.Application.Features.Fees.DTOs.Concession;

public sealed class ConcessionExportRow
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public long? FeeHeadId { get; set; }
    public bool IsAutoApply { get; set; }
    public bool ApprovalRequired { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime CreatedAt { get; set; }
}
