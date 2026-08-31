using MediatR;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.API.Controllers;
using SchoolERP.Application.Common.DTOs;
using SchoolERP.Application.Features.Branches.Commands.BulkDelete;
using SchoolERP.Application.Features.Branches.Commands.BulkPatch;
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

[ApiController]
[Route("api/branches")]
public class BranchesController : BaseApiController
{
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateBranchRequest request)
        => HandleResult(
            await Mediator.Send(new CreateBranchCommand(request)));

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] PagedRequest request)
        => HandleResult(
            await Mediator.Send(new GetBranchesQuery(request)));

    [HttpGet("all")]
    public async Task<IActionResult> GetAllLight()
        => HandleResult(
            await Mediator.Send(new GetBranchesLightQuery()));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
        => HandleResult(
            await Mediator.Send(new GetBranchByIdQuery(id)));

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateBranchRequest request)
        => HandleResult(
            await Mediator.Send(new UpdateBranchCommand(id, request)));

    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> Patch(
        Guid id,
        [FromBody] PatchBranchRequest request)
        => HandleResult(
            await Mediator.Send(new PatchBranchCommand(id, request)));

    [HttpPatch("bulk")]
    public async Task<IActionResult> BulkPatch(
        [FromBody] BulkPatchBranchRequest request)
        => HandleResult(
            await Mediator.Send(new BulkPatchBranchCommand(request)));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
        => HandleResult(
            await Mediator.Send(new DeleteBranchCommand(id)));

    [HttpPost("{id:guid}/restore")]
    public async Task<IActionResult> Restore(Guid id)
        => HandleResult(
            await Mediator.Send(new RestoreBranchCommand(id)));

    [HttpDelete("bulk")]
    public async Task<IActionResult> BulkDelete(
        [FromBody] BulkDeleteBranchRequest request)
        => HandleResult(
            await Mediator.Send(new BulkDeleteBranchCommand(request)));

    [HttpPut("bulk")]
    public async Task<IActionResult> BulkUpdate(
        [FromBody] BulkUpdateBranchRequest request)
        => HandleResult(
            await Mediator.Send(new BulkUpdateBranchCommand(request)));

    [HttpGet("export")]
    public async Task<IActionResult> Export()
    {
        var result = await Mediator.Send(new ExportBranchesQuery());

        if (result.IsFailure)
            return HandleResult(result);

        var fileName = $"branches_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";

        return File(
            result.Value!,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName);
    }
}