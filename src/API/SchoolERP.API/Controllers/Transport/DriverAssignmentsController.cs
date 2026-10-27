using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.Application.Features.Transport.Commands.DriverAssignment.AssignDriverToVehicle;
using SchoolERP.Application.Features.Transport.Commands.DriverAssignment.EndDriverAssignment;
using SchoolERP.Application.Features.Transport.DTOs.DriverAssignment;
using SchoolERP.Application.Features.Transport.Queries.Assignment.GetVehicleDriverHistory;

namespace SchoolERP.API.Controllers.Transport;

/// <summary>
/// TEMPORARY: [AllowAnonymous]. When RBAC ready, replace with
/// [HasPermission(PermissionNames.TransportDriverAssignmentXxx)].
/// </summary>
[ApiController]
[AllowAnonymous]
public sealed class DriverAssignmentsController : BaseApiController
{
    private readonly IMediator _mediator;
    public DriverAssignmentsController(IMediator mediator) => _mediator = mediator;

    [HttpPost("api/transport/vehicles/{vehicleId:long}/driver-assignments")]
    [AllowAnonymous]  // TODO: [HasPermission(PermissionNames.TransportDriverAssignmentAssign)]
    public async Task<IActionResult> Assign([FromRoute] long vehicleId, [FromBody] AssignDriverRequest request, CancellationToken ct)
        => HandleResult(await _mediator.Send(new AssignDriverToVehicleCommand(vehicleId, request), ct));

    [HttpGet("api/transport/vehicles/{vehicleId:long}/driver-assignments/history")]
    [AllowAnonymous]  // TODO: [HasPermission(PermissionNames.TransportDriverAssignmentRead)]
    public async Task<IActionResult> VehicleHistory([FromRoute] long vehicleId, CancellationToken ct)
        => HandleResult(await _mediator.Send(new GetVehicleDriverHistoryQuery(VehicleId: vehicleId), ct));

    [HttpGet("api/transport/drivers/{driverStaffId:long}/assignment-history")]
    [AllowAnonymous]  // TODO: [HasPermission(PermissionNames.TransportDriverAssignmentRead)]
    public async Task<IActionResult> DriverHistory([FromRoute] long driverStaffId, CancellationToken ct)
        => HandleResult(await _mediator.Send(new GetVehicleDriverHistoryQuery(DriverStaffId: driverStaffId), ct));

    [HttpPost("api/transport/driver-assignments/{id:long}/end")]
    [AllowAnonymous]  // TODO: [HasPermission(PermissionNames.TransportDriverAssignmentEnd)]
    public async Task<IActionResult> End([FromRoute] long id, [FromBody] EndDriverAssignmentRequest request, CancellationToken ct)
        => HandleResult(await _mediator.Send(new EndDriverAssignmentCommand(id, request), ct));
}
