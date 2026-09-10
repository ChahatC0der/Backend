using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using SchoolERP.Application.Common.DTOs;

using SchoolERP.Application.Features.Academic.Commands.Class.CreateClass;

using SchoolERP.Application.Features.Academic.DTOs;

using SchoolERP.Application.Features.Academic.Queries.Class.GetClasses;


namespace SchoolERP.API.Controllers;

[ApiController]
[Route("api/classes")]
public class ClassesController : BaseApiController
{
    private readonly IMediator _mediator;

    public ClassesController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [AllowAnonymous] // TODO: [HasPermission("class.read")]
    public async Task<IActionResult> GetClasses([FromQuery] PagedRequest request)
        => HandleResult(await _mediator.Send(new GetClassesQuery(request)));

    [HttpGet("{id:long}")]
    [AllowAnonymous] // TODO: [HasPermission("class.read")]
    public async Task<IActionResult> GetClassById(long id)
        => HandleResult(await _mediator.Send(new GetClassByIdQuery(id)));

    [HttpGet("all")]
    [AllowAnonymous] // TODO: [HasPermission("class.read")]
    public async Task<IActionResult> GetClassesLight()
        => HandleResult(await _mediator.Send(new GetClassesLightQuery()));

    [HttpGet("export")]
    [AllowAnonymous] // TODO: [HasPermission("class.export")]
    public async Task<IActionResult> ExportClasses()
    {
        var result = await _mediator.Send(new ExportClassesQuery());
        if (result.IsFailure) return HandleResult(result);
        return File(result.Value, "text/csv", "classes.csv");
    }

    [HttpPost]
    [AllowAnonymous] // TODO: [HasPermission("class.create")]
    public async Task<IActionResult> CreateClass([FromBody] CreateClassRequest request)
        => HandleResult(await _mediator.Send(new CreateClassCommand(request)));

    [HttpPut("{id:long}")]
    [AllowAnonymous] // TODO: [HasPermission("class.update")]
    public async Task<IActionResult> UpdateClass(long id, [FromBody] UpdateClassRequest request)
    {
        if (id != request.Id) return BadRequest("Id mismatch.");
        return HandleResult(await _mediator.Send(new UpdateClassCommand(request)));
    }

    [HttpPatch("{id:long}")]
    [AllowAnonymous] // TODO: [HasPermission("class.update")]
    public async Task<IActionResult> PatchClass(long id, [FromBody] PatchClassRequest request)
    {
        if (id != request.Id) return BadRequest("Id mismatch.");
        return HandleResult(await _mediator.Send(new PatchClassCommand(request)));
    }

    [HttpDelete("{id:long}")]
    [AllowAnonymous] // TODO: [HasPermission("class.delete")]
    public async Task<IActionResult> DeleteClass(long id)
        => HandleResult(await _mediator.Send(new DeleteClassCommand(id)));

    [HttpPost("{id:long}/restore")]
    [AllowAnonymous] // TODO: [HasPermission("class.update")]
    public async Task<IActionResult> RestoreClass(long id)
        => HandleResult(await _mediator.Send(new RestoreClassCommand(id)));

    [HttpPut("bulk")]
    [AllowAnonymous] // TODO: [HasPermission("class.update")]
    public async Task<IActionResult> BulkUpdateClass([FromBody] BulkUpdateClassRequest request)
        => HandleResult(await _mediator.Send(new BulkUpdateClassCommand(request)));

    [HttpPatch("bulk")]
    [AllowAnonymous] // TODO: [HasPermission("class.update")]
    public async Task<IActionResult> BulkPatchClass([FromBody] BulkPatchClassRequest request)
        => HandleResult(await _mediator.Send(new BulkPatchClassCommand(request)));

    [HttpDelete("bulk")]
    [AllowAnonymous] // TODO: [HasPermission("class.delete")]
    public async Task<IActionResult> BulkDeleteClass([FromBody] BulkDeleteClassRequest request)
        => HandleResult(await _mediator.Send(new BulkDeleteClassCommand(request)));
}