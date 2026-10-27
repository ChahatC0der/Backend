using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Transport.DTOs.DriverAssignment;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Transport.Commands.DriverAssignment.AssignDriverToVehicle;

public record AssignDriverToVehicleCommand(long VehicleId, AssignDriverRequest Request) : ICommand<DriverAssignmentResponse>;
