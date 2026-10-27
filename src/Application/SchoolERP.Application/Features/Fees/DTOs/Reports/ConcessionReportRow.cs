namespace SchoolERP.Application.Features.Fees.DTOs.Reports;

public sealed class ConcessionReportRow
{
    public long ConcessionId { get; set; }
    public string ConcessionName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public int ApprovedStudents { get; set; }
    public decimal TotalApplied { get; set; }
}
