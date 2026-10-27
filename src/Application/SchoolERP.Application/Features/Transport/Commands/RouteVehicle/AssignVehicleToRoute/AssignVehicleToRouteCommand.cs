using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Transport.DTOs.RouteVehicle;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Transport.Commands.RouteVehicle.AssignVehicleToRoute;

public record AssignVehicleToRouteCommand(long RouteId, AssignVehicleToRouteRequest Request) : ICommand<RouteVehicleAssignmentResponse>;
