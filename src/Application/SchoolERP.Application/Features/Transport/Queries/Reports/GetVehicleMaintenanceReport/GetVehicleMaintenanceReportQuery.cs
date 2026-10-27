using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Transport.DTOs.Reports;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Transport.Queries.Reports.GetVehicleMaintenanceReport;

public sealed record GetVehicleMaintenanceReportQuery(long? VehicleId = null, DateTime? From = null, DateTime? To = null) : IQuery<IReadOnlyList<VehicleMaintenanceReportRow>>;
