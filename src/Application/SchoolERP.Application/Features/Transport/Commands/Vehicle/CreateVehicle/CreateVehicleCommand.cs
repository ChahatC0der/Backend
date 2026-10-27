using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Transport.DTOs.Vehicle;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Transport.Commands.Vehicle.CreateVehicle;

public record CreateVehicleCommand(CreateVehicleRequest Request) : ICommand<VehicleResponse>;
