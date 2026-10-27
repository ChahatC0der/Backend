using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Transport.DTOs.VehicleMaintenance;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Transport.Queries.Vehicle.GetVehicleMaintenance;

public sealed record GetVehicleMaintenanceQuery(long VehicleId) : IQuery<IReadOnlyList<VehicleMaintenanceResponse>>;
