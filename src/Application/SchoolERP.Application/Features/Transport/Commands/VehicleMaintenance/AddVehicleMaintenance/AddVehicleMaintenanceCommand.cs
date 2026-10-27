using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Transport.DTOs.VehicleMaintenance;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Transport.Commands.VehicleMaintenance.AddVehicleMaintenance;

public record AddVehicleMaintenanceCommand(long VehicleId, CreateVehicleMaintenanceRequest Request) : ICommand<VehicleMaintenanceResponse>;
