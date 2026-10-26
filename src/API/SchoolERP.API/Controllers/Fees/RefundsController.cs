using MediatR;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.API.Authorization;
using SchoolERP.Application.Features.Fees.Commands.Refund.ApproveRefund;
using SchoolERP.Application.Features.Fees.Commands.Refund.CompleteRefund;
using SchoolERP.Application.Features.Fees.Commands.Refund.RejectRefund;
using SchoolERP.Application.Features.Fees.Commands.Refund.RequestRefund;
using SchoolERP.Application.Features.Fees.DTOs.Refund;
using SchoolERP.Application.Features.Fees.Queries.Refund.GetRefunds;
using SchoolERP.Domain.Shared.Permissions;

namespace SchoolERP.API.Controllers.Fees;

[ApiController]
[Route("api/fees/refunds")]
public sealed class RefundsController : BaseApiController
{
    private readonly IMediator _mediator;
    public RefundsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [HasPermission(PermissionNames.FeeRefundRead)]
    public async Task<IActionResult> Get([FromQuery] GetRefundsQuery query, CancellationToken ct)
        => HandleResult(await _mediator.Send(query, ct));

    [HttpPost("request")]
    [HasPermission(PermissionNames.FeeRefundRequest)]
    public async Task<IActionResult> Request([FromBody] RequestRefundRequest request, CancellationToken ct)
        => HandleResult(await _mediator.Send(new RequestRefundCommand(request), ct));

    [HttpPost("{id:long}/approve")]
    [HasPermission(PermissionNames.FeeRefundApprove)]
    public async Task<IActionResult> Approve([FromRoute] long id, CancellationToken ct)
        => HandleResult(await _mediator.Send(new ApproveRefundCommand(id), ct));

    [HttpPost("{id:long}/complete")]
    [HasPermission(PermissionNames.FeeRefundComplete)]
    public async Task<IActionResult> Complete([FromRoute] long id, CancellationToken ct)
        => HandleResult(await _mediator.Send(new CompleteRefundCommand(id), ct));

    [HttpPost("{id:long}/reject")]
    [HasPermission(PermissionNames.FeeRefundReject)]
    public async Task<IActionResult> Reject([FromRoute] long id, [FromBody] RejectRefundRequest request, CancellationToken ct)
        => HandleResult(await _mediator.Send(new RejectRefundCommand(id, request), ct));
}
