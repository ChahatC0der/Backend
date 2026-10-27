using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Transport.Commands.Route.CreateRoute;
using SchoolERP.Application.Features.Transport.Commands.Route.DeleteRoute;
using SchoolERP.Application.Features.Transport.Commands.Route.PatchRouteStatus;
using SchoolERP.Application.Features.Transport.Commands.Route.UpdateRoute;
using SchoolERP.Application.Features.Transport.Commands.Stop.CreateStop;
using SchoolERP.Application.Features.Transport.Commands.Stop.DeleteStop;
using SchoolERP.Application.Features.Transport.Commands.Stop.ReorderStops;
using SchoolERP.Application.Features.Transport.Commands.Stop.UpdateStop;
using SchoolERP.Application.Features.Transport.DTOs.Route;
using SchoolERP.Application.Features.Transport.DTOs.Stop;
using SchoolERP.Application.Features.Transport.Queries.Route.GetRouteById;
using SchoolERP.Application.Features.Transport.Queries.Route.GetRoutes;
using SchoolERP.Application.Features.Transport.Queries.Route.GetRoutesLight;
using SchoolERP.Application.Features.Transport.Queries.Route.GetStopsByRoute;

namespace SchoolERP.API.Controllers.Transport;

/// <summary>
/// TEMPORARY: All actions use [AllowAnonymous] per current development directive.
/// When RBAC ready, replace [AllowAnonymous] with [HasPermission(PermissionNames.TransportRouteXxx)].
/// Permission constants already exist in PermissionNames.cs.
/// </summary>
[ApiController]
[Route("api/transport/routes")]
[AllowAnonymous]
public sealed class RoutesController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly IExcelExportService _excel;
    public RoutesController(IMediator mediator, IExcelExportService excel)
    { _mediator = mediator; _excel = excel; }

    // ----- Reads -----

    [HttpGet]
    [AllowAnonymous]  // TODO: [HasPermission(PermissionNames.TransportRouteRead)]
    public async Task<IActionResult> Get([FromQuery] GetRoutesQuery query, CancellationToken ct)
        => HandleResult(await _mediator.Send(query, ct));

    [HttpGet("light")]
    [AllowAnonymous]  // TODO: [HasPermission(PermissionNames.TransportRouteRead)]
    public async Task<IActionResult> GetLight(CancellationToken ct)
        => HandleResult(await _mediator.Send(new GetRoutesLightQuery(), ct));

    [HttpGet("{id:long}")]
    [AllowAnonymous]  // TODO: [HasPermission(PermissionNames.TransportRouteRead)]
    public async Task<IActionResult> GetById([FromRoute] long id, CancellationToken ct)
        => HandleResult(await _mediator.Send(new GetRouteByIdQuery(id), ct));

    [HttpGet("{routeId:long}/stops")]
    [AllowAnonymous]  // TODO: [HasPermission(PermissionNames.TransportStopRead)]
    public async Task<IActionResult> GetStops([FromRoute] long routeId, CancellationToken ct)
        => HandleResult(await _mediator.Send(new GetStopsByRouteQuery(routeId), ct));

    // ----- Route writes -----

    [HttpPost]
    [AllowAnonymous]  // TODO: [HasPermission(PermissionNames.TransportRouteCreate)]
    public async Task<IActionResult> Create([FromBody] CreateRouteRequest request, CancellationToken ct)
        => HandleResult(await _mediator.Send(new CreateRouteCommand(request), ct));

    [HttpPut("{id:long}")]
    [AllowAnonymous]  // TODO: [HasPermission(PermissionNames.TransportRouteUpdate)]
    public async Task<IActionResult> Update([FromRoute] long id, [FromBody] UpdateRouteRequest request, CancellationToken ct)
        => HandleResult(await _mediator.Send(new UpdateRouteCommand(id, request), ct));

    [HttpPatch("{id:long}/status")]
    [AllowAnonymous]  // TODO: [HasPermission(PermissionNames.TransportRouteUpdate)]
    public async Task<IActionResult> PatchStatus([FromRoute] long id, [FromBody] PatchRouteStatusRequest request, CancellationToken ct)
        => HandleResult(await _mediator.Send(new PatchRouteStatusCommand(id, request), ct));

    [HttpDelete("{id:long}")]
    [AllowAnonymous]  // TODO: [HasPermission(PermissionNames.TransportRouteDelete)]
    public async Task<IActionResult> Delete([FromRoute] long id, CancellationToken ct)
        => HandleResult(await _mediator.Send(new DeleteRouteCommand(id), ct));

    // ----- Stop writes -----

    [HttpPost("{routeId:long}/stops")]
    [AllowAnonymous]  // TODO: [HasPermission(PermissionNames.TransportStopCreate)]
    public async Task<IActionResult> CreateStop([FromRoute] long routeId, [FromBody] CreateStopRequest request, CancellationToken ct)
        => HandleResult(await _mediator.Send(new CreateStopCommand(routeId, request), ct));

    [HttpPut("{routeId:long}/stops/{stopId:long}")]
    [AllowAnonymous]  // TODO: [HasPermission(PermissionNames.TransportStopUpdate)]
    public async Task<IActionResult> UpdateStop([FromRoute] long routeId, [FromRoute] long stopId, [FromBody] UpdateStopRequest request, CancellationToken ct)
        => HandleResult(await _mediator.Send(new UpdateStopCommand(routeId, stopId, request), ct));

    [HttpPost("{routeId:long}/stops/reorder")]
    [AllowAnonymous]  // TODO: [HasPermission(PermissionNames.TransportStopUpdate)]
    public async Task<IActionResult> ReorderStops([FromRoute] long routeId, [FromBody] ReorderStopsRequest request, CancellationToken ct)
        => HandleResult(await _mediator.Send(new ReorderStopsCommand(routeId, request), ct));

    [HttpDelete("{routeId:long}/stops/{stopId:long}")]
    [AllowAnonymous]  // TODO: [HasPermission(PermissionNames.TransportStopDelete)]
    public async Task<IActionResult> DeleteStop([FromRoute] long routeId, [FromRoute] long stopId, CancellationToken ct)
        => HandleResult(await _mediator.Send(new DeleteStopCommand(routeId, stopId), ct));
}
