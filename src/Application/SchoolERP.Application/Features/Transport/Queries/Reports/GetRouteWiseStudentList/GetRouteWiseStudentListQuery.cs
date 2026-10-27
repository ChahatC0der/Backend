using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Transport.DTOs.Reports;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Transport.Queries.Reports.GetRouteWiseStudentList;

public sealed record GetRouteWiseStudentListQuery(long? RouteId = null, long? AcademicYearId = null) : IQuery<IReadOnlyList<RouteWiseStudentRow>>;
