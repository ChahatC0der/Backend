using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Transport.DTOs.Vehicle;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Transport.Commands.Vehicle.UpdateVehicle;

public record UpdateVehicleCommand(long Id, UpdateVehicleRequest Request) : ICommand<VehicleResponse>;
