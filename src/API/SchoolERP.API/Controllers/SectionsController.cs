using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.Application.Common.DTOs;
using SchoolERP.Application.Features.Academic.Commands.Section.CreateSection;

using SchoolERP.Application.Features.Academic.DTOs;

using SchoolERP.Application.Features.Academic.Queries.Section.GetSections;

namespace SchoolERP.API.Controllers;

[ApiController]
[Route("api/sections")]
public class SectionsController : BaseApiController
{
    private readonly IMediator _mediator;

    public SectionsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [AllowAnonymous] // TODO: [HasPermission("section.read")]
    public async Task<IActionResult> GetSections([FromQuery] PagedRequest request)
        => HandleResult(await _mediator.Send(new GetSectionsQuery(request)));

    [HttpGet("{id:long}")]
    [AllowAnonymous] // TODO: [HasPermission("section.read")]
    public async Task<IActionResult> GetSectionById(long id)
        => HandleResult(await _mediator.Send(new GetSectionByIdQuery(id)));

    [HttpGet("all")]
    [AllowAnonymous] // TODO: [HasPermission("section.read")]
    public async Task<IActionResult> GetSectionsLight()
        => HandleResult(await _mediator.Send(new GetSectionsLightQuery()));

    [HttpGet("export")]
    [AllowAnonymous] // TODO: [HasPermission("section.export")]
    public async Task<IActionResult> ExportSections()
    {
        var result = await _mediator.Send(new ExportSectionsQuery());
        if (result.IsFailure) return HandleResult(result);
        return File(result.Value, "text/csv", "sections.csv");
    }

    [HttpPost]
    [AllowAnonymous] // TODO: [HasPermission("section.create")]
    public async Task<IActionResult> CreateSection([FromBody] CreateSectionRequest request)
        => HandleResult(await _mediator.Send(new CreateSectionCommand(request)));

    [HttpPut("{id:long}")]
    [AllowAnonymous] // TODO: [HasPermission("section.update")]
    public async Task<IActionResult> UpdateSection(long id, [FromBody] UpdateSectionRequest request)
    {
        if (id != request.Id) return BadRequest("Id mismatch.");
        return HandleResult(await _mediator.Send(new UpdateSectionCommand(request)));
    }

    [HttpPatch("{id:long}")]
    [AllowAnonymous] // TODO: [HasPermission("section.update")]
    public async Task<IActionResult> PatchSection(long id, [FromBody] PatchSectionRequest request)
    {
        if (id != request.Id) return BadRequest("Id mismatch.");
        return HandleResult(await _mediator.Send(new PatchSectionCommand(request)));
    }

    [HttpDelete("{id:long}")]
    [AllowAnonymous] // TODO: [HasPermission("section.delete")]
    public async Task<IActionResult> DeleteSection(long id)
        => HandleResult(await _mediator.Send(new DeleteSectionCommand(id)));

    [HttpPost("{id:long}/restore")]
    [AllowAnonymous] // TODO: [HasPermission("section.update")]
    public async Task<IActionResult> RestoreSection(long id)
        => HandleResult(await _mediator.Send(new RestoreSectionCommand(id)));

    [HttpPut("bulk")]
    [AllowAnonymous] // TODO: [HasPermission("section.update")]
    public async Task<IActionResult> BulkUpdateSection([FromBody] BulkUpdateSectionRequest request)
        => HandleResult(await _mediator.Send(new BulkUpdateSectionCommand(request)));

    [HttpPatch("bulk")]
    [AllowAnonymous] // TODO: [HasPermission("section.update")]
    public async Task<IActionResult> BulkPatchSection([FromBody] BulkPatchSectionRequest request)
        => HandleResult(await _mediator.Send(new BulkPatchSectionCommand(request)));

    [HttpDelete("bulk")]
    [AllowAnonymous] // TODO: [HasPermission("section.delete")]
    public async Task<IActionResult> BulkDeleteSection([FromBody] BulkDeleteSectionRequest request)
        => HandleResult(await _mediator.Send(new BulkDeleteSectionCommand(request)));
}