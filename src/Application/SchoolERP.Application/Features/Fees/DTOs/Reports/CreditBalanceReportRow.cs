namespace SchoolERP.Application.Features.Fees.DTOs.Reports;

public sealed class CreditBalanceReportRow
{
    public long StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string EnrollmentId { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public DateTime LastUpdatedAt { get; set; }
}
