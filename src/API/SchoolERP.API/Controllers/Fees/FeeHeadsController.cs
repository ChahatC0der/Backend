using MediatR;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.API.Authorization;
using SchoolERP.Application.Features.Fees.Commands.FeeHead.BulkDeleteFeeHead;
using SchoolERP.Application.Features.Fees.Commands.FeeHead.CreateFeeHead;
using SchoolERP.Application.Features.Fees.Commands.FeeHead.DeleteFeeHead;
using SchoolERP.Application.Features.Fees.Commands.FeeHead.PatchFeeHead;
using SchoolERP.Application.Features.Fees.Commands.FeeHead.RestoreFeeHead;
using SchoolERP.Application.Features.Fees.Commands.FeeHead.SeedDefaultFeeHeads;
using SchoolERP.Application.Features.Fees.Commands.FeeHead.UpdateFeeHead;
using SchoolERP.Application.Features.Fees.DTOs.FeeHead;
using SchoolERP.Application.Features.Fees.Queries.FeeHead.ExportFeeHeads;
using SchoolERP.Application.Features.Fees.Queries.FeeHead.GetFeeHeadById;
using SchoolERP.Application.Features.Fees.Queries.FeeHead.GetFeeHeads;
using SchoolERP.Application.Features.Fees.Queries.FeeHead.GetFeeHeadsLight;
using SchoolERP.Domain.Shared.Permissions;

namespace SchoolERP.API.Controllers.Fees;

[ApiController]
[Route("api/fees/heads")]
public sealed class FeeHeadsController : BaseApiController
{
    private const string ExcelMime =
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    private readonly IMediator _mediator;
    public FeeHeadsController(IMediator mediator) => _mediator = mediator;

    // ----- Reads -----

    [HttpGet]
    [HasPermission(PermissionNames.FeeHeadRead)]
    public async Task<IActionResult> Get([FromQuery] GetFeeHeadsQuery query, CancellationToken ct)
        => HandleResult(await _mediator.Send(query, ct));

    [HttpGet("light")]
    [HasPermission(PermissionNames.FeeHeadRead)]
    public async Task<IActionResult> GetLight(CancellationToken ct)
        => HandleResult(await _mediator.Send(new GetFeeHeadsLightQuery(), ct));

    [HttpGet("{id:long}")]
    [HasPermission(PermissionNames.FeeHeadRead)]
    public async Task<IActionResult> GetById([FromRoute] long id, CancellationToken ct)
        => HandleResult(await _mediator.Send(new GetFeeHeadByIdQuery(id), ct));

    // ----- Writes -----

    [HttpPost]
    [HasPermission(PermissionNames.FeeHeadCreate)]
    public async Task<IActionResult> Create([FromBody] CreateFeeHeadRequest request, CancellationToken ct)
        => HandleResult(await _mediator.Send(new CreateFeeHeadCommand(request), ct));

    [HttpPost("seed-defaults")]
    [HasPermission(PermissionNames.FeeHeadSeedDefaults)]
    public async Task<IActionResult> SeedDefaults(CancellationToken ct)
        => HandleResult(await _mediator.Send(new SeedDefaultFeeHeadsCommand(), ct));

    [HttpPut("{id:long}")]
    [HasPermission(PermissionNames.FeeHeadUpdate)]
    public async Task<IActionResult> Update([FromRoute] long id, [FromBody] UpdateFeeHeadRequest request, CancellationToken ct)
        => HandleResult(await _mediator.Send(new UpdateFeeHeadCommand(id, request), ct));

    [HttpPatch("{id:long}")]
    [HasPermission(PermissionNames.FeeHeadUpdate)]
    public async Task<IActionResult> Patch([FromRoute] long id, [FromBody] PatchFeeHeadRequest request, CancellationToken ct)
        => HandleResult(await _mediator.Send(new PatchFeeHeadCommand(id, request), ct));

    [HttpDelete("{id:long}")]
    [HasPermission(PermissionNames.FeeHeadDelete)]
    public async Task<IActionResult> Delete([FromRoute] long id, CancellationToken ct)
        => HandleResult(await _mediator.Send(new DeleteFeeHeadCommand(id), ct));

    [HttpPost("{id:long}/restore")]
    [HasPermission(PermissionNames.FeeHeadRestore)]
    public async Task<IActionResult> Restore([FromRoute] long id, CancellationToken ct)
        => HandleResult(await _mediator.Send(new RestoreFeeHeadCommand(id), ct));

    [HttpDelete("bulk")]
    [HasPermission(PermissionNames.FeeHeadBulkDelete)]
    public async Task<IActionResult> BulkDelete([FromBody] BulkDeleteFeeHeadCommand command, CancellationToken ct)
        => HandleResult(await _mediator.Send(command, ct));

    // ----- Export -----

    [HttpGet("export")]
    [HasPermission(PermissionNames.FeeHeadExport)]
    public async Task<IActionResult> Export([FromQuery] ExportFeeHeadsQuery query, CancellationToken ct)
    {
        var result = await _mediator.Send(query, ct);
        if (result.IsFailure) return HandleResult(result);
        var filename = $"FeeHeads_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";
        return File(result.Value, ExcelMime, filename);
    }
}
