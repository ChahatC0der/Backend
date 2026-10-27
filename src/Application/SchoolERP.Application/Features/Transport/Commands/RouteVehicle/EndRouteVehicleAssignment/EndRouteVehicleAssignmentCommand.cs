using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Transport.DTOs.RouteVehicle;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Transport.Commands.RouteVehicle.EndRouteVehicleAssignment;

public record EndRouteVehicleAssignmentCommand(long AssignmentId, EndRouteVehicleAssignmentRequest Request) : ICommand<RouteVehicleAssignmentResponse>;
