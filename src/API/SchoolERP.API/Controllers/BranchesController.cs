using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.Application.Common.DTOs;
using SchoolERP.Application.Features.Branches.Commands.BulkDelete;
using SchoolERP.Application.Features.Branches.Commands.BulkUpdate;
using SchoolERP.Application.Features.Branches.Commands.CreateBranch;
using SchoolERP.Application.Features.Branches.Commands.DeleteBranch;
using SchoolERP.Application.Features.Branches.Commands.PatchBranch;
using SchoolERP.Application.Features.Branches.Commands.RestoreBranch;
using SchoolERP.Application.Features.Branches.Commands.UpdateBranch;
using SchoolERP.Application.Features.Branches.DTOs;
using SchoolERP.Application.Features.Branches.Queries.ExportBranches;
using SchoolERP.Application.Features.Branches.Queries.GetBranchById;
using SchoolERP.Application.Features.Branches.Queries.GetBranches;
using SchoolERP.Application.Features.Branches.Queries.GetBranchesLight;

namespace SchoolERP.API.Controllers;

[ApiController]
[Route("api/tenants/{tenantId:guid}/branches")]
public class BranchesController : BaseApiController
{
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Create(Guid tenantId, [FromBody] CreateBranchRequest request)
        => HandleResult(await Mediator.Send(new CreateBranchCommand(tenantId, request)));

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(Guid tenantId, [FromQuery] PagedRequest request)
        => HandleResult(await Mediator.Send(new GetBranchesQuery(tenantId, request)));

    [HttpGet("all")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAllLight(Guid tenantId)
        => HandleResult(await Mediator.Send(new GetBranchesLightQuery(tenantId)));

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid tenantId, Guid id)
        => HandleResult(await Mediator.Send(new GetBranchByIdQuery(tenantId, id)));

    [HttpPut("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> Update(Guid tenantId, Guid id, [FromBody] UpdateBranchRequest request)
        => HandleResult(await Mediator.Send(new UpdateBranchCommand(tenantId, id, request)));

    [HttpPatch("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> Patch(Guid tenantId, Guid id, [FromBody] PatchBranchRequest request)
        => HandleResult(await Mediator.Send(new PatchBranchCommand(tenantId, id, request)));

    [HttpDelete("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> Delete(Guid tenantId, Guid id)
        => HandleResult(await Mediator.Send(new DeleteBranchCommand(tenantId, id)));

    [HttpPost("{id:guid}/restore")]
    [AllowAnonymous]
    public async Task<IActionResult> Restore(Guid tenantId, Guid id)
        => HandleResult(await Mediator.Send(new RestoreBranchCommand(tenantId, id)));

    [HttpDelete("bulk")]
    [AllowAnonymous]
    public async Task<IActionResult> BulkDelete(Guid tenantId, [FromBody] BulkDeleteBranchRequest request)
        => HandleResult(await Mediator.Send(new BulkDeleteBranchCommand(tenantId, request)));

    [HttpPut("bulk")]
    [AllowAnonymous]
    public async Task<IActionResult> BulkUpdate(Guid tenantId, [FromBody] BulkUpdateBranchRequest request)
        => HandleResult(await Mediator.Send(new BulkUpdateBranchCommand(tenantId, request)));

    [HttpGet("export")]
    [AllowAnonymous]
    public async Task<IActionResult> Export(Guid tenantId)
    {
        var result = await Mediator.Send(new ExportBranchesQuery(tenantId));
        if (result.IsFailure)
            return HandleResult(result);

        var fileName = $"branches_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";
        return File(result.Value!, "text/csv", fileName);
    }
}