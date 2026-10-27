using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Transport.DTOs.Reports;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Transport.Queries.Reports.GetDriverReport;

public sealed record GetDriverReportQuery(bool? OnlyCurrent = true) : IQuery<IReadOnlyList<DriverReportRow>>;
