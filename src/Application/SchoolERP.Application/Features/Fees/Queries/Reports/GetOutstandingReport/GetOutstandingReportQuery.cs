using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Fees.DTOs.Reports;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Fees.Queries.Reports.GetOutstandingReport;

public sealed class GetOutstandingReportQuery : IQuery<IReadOnlyList<OutstandingReportRow>>
{
    public long? AcademicYearId { get; set; }
    public long? ClassId { get; set; }
}
