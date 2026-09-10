using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.Application.Common.DTOs;
using SchoolERP.Application.Features.Academic.Commands.ClassGroup.CreateClassGroup;
using SchoolERP.Application.Features.Academic.DTOs;
using SchoolERP.Application.Features.Academic.Queries.ClassGroup.GetClassGroups;


namespace SchoolERP.API.Controllers;

[ApiController]
[Route("api/class-groups")]
public class ClassGroupsController : BaseApiController
{
    private readonly IMediator _mediator;

    public ClassGroupsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [AllowAnonymous] // TODO: [HasPermission("class_group.read")]
    public async Task<IActionResult> GetClassGroups([FromQuery] PagedRequest request)
        => HandleResult(await _mediator.Send(new GetClassGroupsQuery(request)));

    [HttpGet("{id:long}")]
    [AllowAnonymous] // TODO: [HasPermission("class_group.read")]
    public async Task<IActionResult> GetClassGroupById(long id)
        => HandleResult(await _mediator.Send(new GetClassGroupByIdQuery(id)));

    [HttpGet("all")]
    [AllowAnonymous] // TODO: [HasPermission("class_group.read")]
    public async Task<IActionResult> GetClassGroupsLight()
        => HandleResult(await _mediator.Send(new GetClassGroupsLightQuery()));

    [HttpGet("export")]
    [AllowAnonymous] // TODO: [HasPermission("class_group.export")]
    public async Task<IActionResult> ExportClassGroups()
    {
        var result = await _mediator.Send(new ExportClassGroupsQuery());
        if (result.IsFailure) return HandleResult(result);
        return File(result.Value, "text/csv", "class_groups.csv");
    }

    [HttpPost]
    [AllowAnonymous] // TODO: [HasPermission("class_group.create")]
    public async Task<IActionResult> CreateClassGroup([FromBody] CreateClassGroupRequest request)
        => HandleResult(await _mediator.Send(new CreateClassGroupCommand(request)));

    [HttpPut("{id:long}")]
    [AllowAnonymous] // TODO: [HasPermission("class_group.update")]
    public async Task<IActionResult> UpdateClassGroup(long id, [FromBody] UpdateClassGroupRequest request)
    {
        if (id != request.Id) return BadRequest("Id mismatch.");
        return HandleResult(await _mediator.Send(new UpdateClassGroupCommand(request)));
    }

    [HttpPatch("{id:long}")]
    [AllowAnonymous] // TODO: [HasPermission("class_group.update")]
    public async Task<IActionResult> PatchClassGroup(long id, [FromBody] PatchClassGroupRequest request)
    {
        if (id != request.Id) return BadRequest("Id mismatch.");
        return HandleResult(await _mediator.Send(new PatchClassGroupCommand(request)));
    }

    [HttpDelete("{id:long}")]
    [AllowAnonymous] // TODO: [HasPermission("class_group.delete")]
    public async Task<IActionResult> DeleteClassGroup(long id)
        => HandleResult(await _mediator.Send(new DeleteClassGroupCommand(id)));

    [HttpPost("{id:long}/restore")]
    [AllowAnonymous] // TODO: [HasPermission("class_group.update")]
    public async Task<IActionResult> RestoreClassGroup(long id)
        => HandleResult(await _mediator.Send(new RestoreClassGroupCommand(id)));

    [HttpPut("bulk")]
    [AllowAnonymous] // TODO: [HasPermission("class_group.update")]
    public async Task<IActionResult> BulkUpdateClassGroup([FromBody] BulkUpdateClassGroupRequest request)
        => HandleResult(await _mediator.Send(new BulkUpdateClassGroupCommand(request)));

    [HttpPatch("bulk")]
    [AllowAnonymous] // TODO: [HasPermission("class_group.update")]
    public async Task<IActionResult> BulkPatchClassGroup([FromBody] BulkPatchClassGroupRequest request)
        => HandleResult(await _mediator.Send(new BulkPatchClassGroupCommand(request)));

    [HttpDelete("bulk")]
    [AllowAnonymous] // TODO: [HasPermission("class_group.delete")]
    public async Task<IActionResult> BulkDeleteClassGroup([FromBody] BulkDeleteClassGroupRequest request)
        => HandleResult(await _mediator.Send(new BulkDeleteClassGroupCommand(request)));
}