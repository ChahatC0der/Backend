using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Transport.DTOs.Vehicle;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Transport.Commands.Vehicle.PatchVehicleStatus;

public record PatchVehicleStatusCommand(long Id, PatchVehicleStatusRequest Request) : ICommand<VehicleResponse>;
