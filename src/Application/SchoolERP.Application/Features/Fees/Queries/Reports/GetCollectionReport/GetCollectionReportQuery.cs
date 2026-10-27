using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Fees.DTOs.Reports;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Fees.Queries.Reports.GetCollectionReport;

public sealed class GetCollectionReportQuery : IQuery<IReadOnlyList<CollectionReportRow>>
{
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
}
