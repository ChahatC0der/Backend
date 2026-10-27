using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.Application.Features.Transport.Commands.TransportFee.CreateTransportFee;
using SchoolERP.Application.Features.Transport.Commands.TransportFee.DeleteTransportFee;
using SchoolERP.Application.Features.Transport.Commands.TransportFee.UpdateTransportFee;
using SchoolERP.Application.Features.Transport.DTOs.TransportFee;
using SchoolERP.Application.Features.Transport.Queries.TransportFee.GetTransportFees;

namespace SchoolERP.API.Controllers.Transport;

/// <summary>
/// TEMPORARY: [AllowAnonymous]. When RBAC ready, replace with
/// [HasPermission(PermissionNames.TransportFeeXxx)].
/// </summary>
[ApiController]
[Route("api/transport/fees")]
[AllowAnonymous]
public sealed class TransportFeesController : BaseApiController
{
    private readonly IMediator _mediator;
    public TransportFeesController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [AllowAnonymous]  // TODO: [HasPermission(PermissionNames.TransportFeeRead)]
    public async Task<IActionResult> Get([FromQuery] GetTransportFeesQuery query, CancellationToken ct)
        => HandleResult(await _mediator.Send(query, ct));

    [HttpPost]
    [AllowAnonymous]  // TODO: [HasPermission(PermissionNames.TransportFeeCreate)]
    public async Task<IActionResult> Create([FromBody] CreateTransportFeeRequest request, CancellationToken ct)
        => HandleResult(await _mediator.Send(new CreateTransportFeeCommand(request), ct));

    [HttpPut("{id:long}")]
    [AllowAnonymous]  // TODO: [HasPermission(PermissionNames.TransportFeeUpdate)]
    public async Task<IActionResult> Update([FromRoute] long id, [FromBody] UpdateTransportFeeRequest request, CancellationToken ct)
        => HandleResult(await _mediator.Send(new UpdateTransportFeeCommand(id, request), ct));

    [HttpDelete("{id:long}")]
    [AllowAnonymous]  // TODO: [HasPermission(PermissionNames.TransportFeeDelete)]
    public async Task<IActionResult> Delete([FromRoute] long id, CancellationToken ct)
        => HandleResult(await _mediator.Send(new DeleteTransportFeeCommand(id), ct));
}
