using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Fees.DTOs.Reports;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Fees.Queries.Reports.GetConcessionReport;

public sealed record GetConcessionReportQuery(long? AcademicYearId = null) : IQuery<IReadOnlyList<ConcessionReportRow>>;
