using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.Application.Common.DTOs;
using SchoolERP.Application.Features.Rbac.Commands.Module.CreateModule;
using SchoolERP.Application.Features.Rbac.Commands.Module.DeleteModule;
using SchoolERP.Application.Features.Rbac.Commands.Module.PatchModule;
using SchoolERP.Application.Features.Rbac.Commands.Module.RestoreModule;
using SchoolERP.Application.Features.Rbac.Commands.Module.UpdateModule;
using SchoolERP.Application.Features.Rbac.DTOs;
using SchoolERP.Application.Features.Rbac.Queries.Module.ExportModules;
using SchoolERP.Application.Features.Rbac.Queries.Module.GetModuleById;
using SchoolERP.Application.Features.Rbac.Queries.Module.GetModules;
using SchoolERP.Application.Features.Rbac.Queries.Module.GetModulesLight;

namespace SchoolERP.API.Controllers;

[ApiController]
[Route("api/modules")]
public class ModulesController : BaseApiController
{
    private readonly IMediator _mediator;

    public ModulesController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [AllowAnonymous] // TODO: [HasPermission("module.read")]
    public async Task<IActionResult> GetModules([FromQuery] PagedRequest request)
        => HandleResult(await _mediator.Send(new GetModulesQuery(request)));

    [HttpGet("{id:long}")]
    [AllowAnonymous] // TODO: [HasPermission("module.read")]
    public async Task<IActionResult> GetModuleById(long id)
        => HandleResult(await _mediator.Send(new GetModuleByIdQuery(id)));

    [HttpGet("all")]
    [AllowAnonymous] // TODO: [HasPermission("module.read")]
    public async Task<IActionResult> GetModulesLight()
        => HandleResult(await _mediator.Send(new GetModulesLightQuery()));

    [HttpGet("export")]
    [AllowAnonymous] // TODO: [HasPermission("module.export")]
    public async Task<IActionResult> ExportModules()
    {
        var result = await _mediator.Send(new ExportModulesQuery());
        if (result.IsFailure) return HandleResult(result);
        return File(result.Value, "text/csv", "modules.csv");
    }

    [HttpPost]
    [AllowAnonymous] // TODO: [HasPermission("module.create")]
    public async Task<IActionResult> CreateModule([FromBody] CreateModuleRequest request)
        => HandleResult(await _mediator.Send(new CreateModuleCommand(request)));

    [HttpPut("{id:long}")]
    [AllowAnonymous] // TODO: [HasPermission("module.update")]
    public async Task<IActionResult> UpdateModule(long id, [FromBody] UpdateModuleRequest request)
    {
        if (id != request.Id) return BadRequest("Id mismatch.");
        return HandleResult(await _mediator.Send(new UpdateModuleCommand(request)));
    }

    [HttpPatch("{id:long}")]
    [AllowAnonymous] // TODO: [HasPermission("module.update")]
    public async Task<IActionResult> PatchModule(long id, [FromBody] PatchModuleRequest request)
    {
        if (id != request.Id) return BadRequest("Id mismatch.");
        return HandleResult(await _mediator.Send(new PatchModuleCommand(request)));
    }

    [HttpDelete("{id:long}")]
    [AllowAnonymous] // TODO: [HasPermission("module.delete")]
    public async Task<IActionResult> DeleteModule(long id)
        => HandleResult(await _mediator.Send(new DeleteModuleCommand(id)));

    [HttpPost("{id:long}/restore")]
    [AllowAnonymous] // TODO: [HasPermission("module.update")]
    public async Task<IActionResult> RestoreModule(long id)
        => HandleResult(await _mediator.Send(new RestoreModuleCommand(id)));
}