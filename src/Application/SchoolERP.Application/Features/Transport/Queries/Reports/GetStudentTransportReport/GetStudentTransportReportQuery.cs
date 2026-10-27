using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Transport.DTOs.Reports;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Transport.Queries.Reports.GetStudentTransportReport;

public sealed record GetStudentTransportReportQuery(long? AcademicYearId = null) : IQuery<IReadOnlyList<StudentTransportReportRow>>;
