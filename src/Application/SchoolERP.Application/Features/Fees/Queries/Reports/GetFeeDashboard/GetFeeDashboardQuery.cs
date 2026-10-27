using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Fees.DTOs.Reports;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Fees.Queries.Reports.GetFeeDashboard;

public sealed class GetFeeDashboardQuery : IQuery<FeeDashboardResponse>
{
    public long? AcademicYearId { get; set; }
}
