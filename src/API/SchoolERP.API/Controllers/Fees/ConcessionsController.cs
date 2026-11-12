using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.Application.Features.Fees.Commands.Concession.AllocateConcession;
using SchoolERP.Application.Features.Fees.Commands.Concession.ApproveConcession;
using SchoolERP.Application.Features.Fees.Commands.Concession.AutoApplySiblingConcessions;
using SchoolERP.Application.Features.Fees.Commands.Concession.CancelAllocation;
using SchoolERP.Application.Features.Fees.Commands.Concession.CreateConcession;
using SchoolERP.Application.Features.Fees.Commands.Concession.DeleteConcession;
using SchoolERP.Application.Features.Fees.Commands.Concession.RejectConcession;
using SchoolERP.Application.Features.Fees.Commands.Concession.UpdateConcession;
using SchoolERP.Application.Features.Fees.DTOs.Concession;
using SchoolERP.Application.Features.Fees.Queries.Concession.ExportConcessions;
using SchoolERP.Application.Features.Fees.Queries.Concession.GetConcessionAllocations;
using SchoolERP.Application.Features.Fees.Queries.Concession.GetConcessionLights;
using SchoolERP.Application.Features.Fees.Queries.Concession.GetConcessions;
using SchoolERP.Application.Features.Fees.Queries.Concession.GetPendingConcessions;
// using SchoolERP.Domain.Shared.Permissions;

namespace SchoolERP.API.Controllers.Fees;

[ApiController]
[AllowAnonymous]
[Route("api/fees/concessions")]
public sealed class ConcessionsController : BaseApiController
{
    private readonly IMediator _mediator;
    public ConcessionsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    // [HasPermission(PermissionNames.FeeConcessionRead)]
    public async Task<IActionResult> Get([FromQuery] GetConcessionsQuery query, CancellationToken ct)
        => HandleResult(await _mediator.Send(query, ct));

    [HttpGet("allocations")]
    // [HasPermission(PermissionNames.FeeConcessionRead)]
    public async Task<IActionResult> GetAllocations([FromQuery] GetConcessionAllocationsQuery query, CancellationToken ct)
    => HandleResult(await _mediator.Send(query, ct));

    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] ExportConcessionsQuery query, CancellationToken ct)
    {
        var result = await _mediator.Send(query, ct);

        if (result.IsFailure)
            return BadRequest(result.Error);   // error handling apne FeeHead export endpoint jaisi karna

        return File(result.Value,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"concessions-{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx");
    }
    [HttpDelete("allocations/{id:long}")]
    public async Task<IActionResult> CancelAllocation(long id, CancellationToken ct)
    {
        var result = await _mediator.Send(new CancelAllocationCommand(id), ct);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(new { message = result.Value });
    }

    [HttpGet("light")]
    public async Task<IActionResult> GetLight(CancellationToken ct)
    => HandleResult(await _mediator.Send(new GetConcessionLightsQuery(), ct));

    [HttpPost]
    // [HasPermission(PermissionNames.FeeConcessionCreate)]
    public async Task<IActionResult> Create([FromBody] CreateConcessionRequest request, CancellationToken ct)
        => HandleResult(await _mediator.Send(new CreateConcessionCommand(request), ct));

    [HttpPut("{id:long}")]
    // [HasPermission(PermissionNames.FeeConcessionUpdate)]
    public async Task<IActionResult> Update([FromRoute] long id, [FromBody] UpdateConcessionRequest request, CancellationToken ct)
        => HandleResult(await _mediator.Send(new UpdateConcessionCommand(id, request), ct));

    [HttpDelete("{id:long}")]
    // [HasPermission(PermissionNames.FeeConcessionDelete)]
    public async Task<IActionResult> Delete([FromRoute] long id, CancellationToken ct)
        => HandleResult(await _mediator.Send(new DeleteConcessionCommand(id), ct));

    [HttpPost("allocate")]
    // [HasPermission(PermissionNames.FeeConcessionAllocate)]
    public async Task<IActionResult> Allocate([FromBody] AllocateConcessionRequest request, CancellationToken ct)
        => HandleResult(await _mediator.Send(new AllocateConcessionCommand(request), ct));

    [HttpPost("auto-detect-siblings")]
    // [HasPermission(PermissionNames.FeeConcessionAutoSibling)]
    public async Task<IActionResult> AutoSibling([FromQuery] long academicYearId, CancellationToken ct)
        => HandleResult(await _mediator.Send(new AutoApplySiblingConcessionsCommand(academicYearId), ct));

    [HttpGet("pending-approvals")]
    // [HasPermission(PermissionNames.FeeConcessionRead)]
    public async Task<IActionResult> GetPending([FromQuery] GetPendingConcessionsQuery query, CancellationToken ct)
        => HandleResult(await _mediator.Send(query, ct));

    [HttpPost("allocations/{id:long}/approve")]
    // [HasPermission(PermissionNames.FeeConcessionApprove)]
    public async Task<IActionResult> Approve([FromRoute] long id, CancellationToken ct)
        => HandleResult(await _mediator.Send(new ApproveConcessionCommand(id), ct));

    [HttpPost("allocations/{id:long}/reject")]
    // [HasPermission(PermissionNames.FeeConcessionReject)]
    public async Task<IActionResult> Reject([FromRoute] long id, [FromBody] RejectConcessionRequest request, CancellationToken ct)
        => HandleResult(await _mediator.Send(new RejectConcessionCommand(id, request), ct));
}