using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Fees.DTOs.Reports;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Fees.Queries.Reports.GetOverdueDefaultersReport;

public sealed class GetOverdueDefaultersReportQuery : IQuery<IReadOnlyList<DefaulterReportRow>>
{
    public int OverdueThresholdDays { get; set; } = 30;
}
