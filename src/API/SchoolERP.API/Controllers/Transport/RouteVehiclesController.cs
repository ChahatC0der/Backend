using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.Application.Features.Transport.Commands.RouteVehicle.AssignVehicleToRoute;
using SchoolERP.Application.Features.Transport.Commands.RouteVehicle.EndRouteVehicleAssignment;
using SchoolERP.Application.Features.Transport.DTOs.RouteVehicle;
using SchoolERP.Application.Features.Transport.Queries.Assignment.GetRouteVehicleHistory;

namespace SchoolERP.API.Controllers.Transport;

/// <summary>
/// TEMPORARY: All actions use [AllowAnonymous]. When RBAC ready, replace with
/// [HasPermission(PermissionNames.TransportRouteVehicleXxx)].
/// </summary>
[ApiController]
[AllowAnonymous]
public sealed class RouteVehiclesController : BaseApiController
{
    private readonly IMediator _mediator;
    public RouteVehiclesController(IMediator mediator) => _mediator = mediator;

    [HttpPost("api/transport/routes/{routeId:long}/vehicles")]
    [AllowAnonymous]  // TODO: [HasPermission(PermissionNames.TransportRouteVehicleAssign)]
    public async Task<IActionResult> Assign([FromRoute] long routeId, [FromBody] AssignVehicleToRouteRequest request, CancellationToken ct)
        => HandleResult(await _mediator.Send(new AssignVehicleToRouteCommand(routeId, request), ct));

    [HttpGet("api/transport/routes/{routeId:long}/vehicles/history")]
    [AllowAnonymous]  // TODO: [HasPermission(PermissionNames.TransportRouteVehicleRead)]
    public async Task<IActionResult> RouteHistory([FromRoute] long routeId, CancellationToken ct)
        => HandleResult(await _mediator.Send(new GetRouteVehicleHistoryQuery(RouteId: routeId), ct));

    [HttpGet("api/transport/vehicles/{vehicleId:long}/route-history")]
    [AllowAnonymous]  // TODO: [HasPermission(PermissionNames.TransportRouteVehicleRead)]
    public async Task<IActionResult> VehicleHistory([FromRoute] long vehicleId, CancellationToken ct)
        => HandleResult(await _mediator.Send(new GetRouteVehicleHistoryQuery(VehicleId: vehicleId), ct));

    [HttpPost("api/transport/route-vehicles/{id:long}/end")]
    [AllowAnonymous]  // TODO: [HasPermission(PermissionNames.TransportRouteVehicleEnd)]
    public async Task<IActionResult> End([FromRoute] long id, [FromBody] EndRouteVehicleAssignmentRequest request, CancellationToken ct)
        => HandleResult(await _mediator.Send(new EndRouteVehicleAssignmentCommand(id, request), ct));
}
