namespace SchoolERP.Application.Features.Fees.DTOs.Reports;

public sealed class FeeDashboardResponse
{
    public decimal CollectionToday { get; set; }
    public decimal CollectionWeek { get; set; }
    public decimal CollectionMonth { get; set; }
    public decimal CollectionYear { get; set; }
    public decimal TotalOutstanding { get; set; }
    public decimal TotalOverdue { get; set; }
    public int OverdueInvoiceCount { get; set; }
    public List<DashboardBreakdown> ByFeeHead { get; set; } = new();
    public List<DashboardBreakdown> ByClass { get; set; } = new();
}

public sealed class DashboardBreakdown
{
    public string Label { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
