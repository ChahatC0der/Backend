using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.Application.Features.Student.Commands.Family.CreateFamily;
using SchoolERP.Application.Features.Student.Commands.Family.DeleteFamily;
using SchoolERP.Application.Features.Student.Commands.Family.LinkStudentToFamily;
using SchoolERP.Application.Features.Student.Commands.Family.PatchFamily;
using SchoolERP.Application.Features.Student.Commands.Family.RestoreFamily;
using SchoolERP.Application.Features.Student.Commands.Family.UnlinkStudentFromFamily;
using SchoolERP.Application.Features.Student.Commands.Family.UpdateFamily;
using SchoolERP.Application.Features.Student.DTOs;
using SchoolERP.Application.Features.Student.Queries.Family.ExportFamilies;
using SchoolERP.Application.Features.Student.Queries.Family.GetFamilies;
using SchoolERP.Application.Features.Student.Queries.Family.GetFamiliesLight;
using SchoolERP.Application.Features.Student.Queries.Family.GetFamilyById;
using SchoolERP.Application.Features.Student.Queries.Family.GetFamilyDetailed;

namespace SchoolERP.API.Controllers.Student;

[ApiController]
[Route("api/families")]
public class FamiliesController : BaseApiController
{
    private readonly IMediator _mediator;

    public FamiliesController(IMediator mediator) => _mediator = mediator;

    // ==================== READ ====================

    [HttpGet]
    [AllowAnonymous] // TODO: [HasPermission("family.read")]
    public async Task<IActionResult> GetFamilies([FromQuery] GetFamiliesRequest request)
        => HandleResult(await _mediator.Send(new GetFamiliesQuery(request)));

    [HttpGet("{id:long}")]
    [AllowAnonymous] // TODO: [HasPermission("family.read")]
    public async Task<IActionResult> GetFamilyById(long id)
        => HandleResult(await _mediator.Send(new GetFamilyByIdQuery(id)));

    [HttpGet("{id:long}/detailed")]
    [AllowAnonymous] // TODO: [HasPermission("family.read")]
    public async Task<IActionResult> GetFamilyDetailed(long id)
        => HandleResult(await _mediator.Send(new GetFamilyDetailedQuery(id)));

    [HttpGet("all")]
    [AllowAnonymous] // TODO: [HasPermission("family.read")]
    public async Task<IActionResult> GetFamiliesLight()
        => HandleResult(await _mediator.Send(new GetFamiliesLightQuery()));

    [HttpGet("export")]
    [AllowAnonymous] // TODO: [HasPermission("family.export")]
    public async Task<IActionResult> ExportFamilies()
    {
        var result = await _mediator.Send(new ExportFamiliesQuery());
        if (result.IsFailure) return HandleResult(result);
        return File(
            result.Value,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"families_{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx"
        );
    }

    // ==================== CRUD ====================

    [HttpPost]
    [AllowAnonymous] // TODO: [HasPermission("family.create")]
    public async Task<IActionResult> CreateFamily([FromBody] CreateFamilyRequest request)
        => HandleResult(await _mediator.Send(new CreateFamilyCommand(request)));

    [HttpPut("{id:long}")]
    [AllowAnonymous] // TODO: [HasPermission("family.update")]
    public async Task<IActionResult> UpdateFamily(long id, [FromBody] UpdateFamilyRequest request)
    {
        if (id != request.Id) return BadRequest("Id mismatch.");
        return HandleResult(await _mediator.Send(new UpdateFamilyCommand(request)));
    }

    [HttpPatch("{id:long}")]
    [AllowAnonymous] // TODO: [HasPermission("family.update")]
    public async Task<IActionResult> PatchFamily(long id, [FromBody] PatchFamilyRequest request)
    {
        if (id != request.Id) return BadRequest("Id mismatch.");
        return HandleResult(await _mediator.Send(new PatchFamilyCommand(request)));
    }

    [HttpDelete("{id:long}")]
    [AllowAnonymous] // TODO: [HasPermission("family.delete")]
    public async Task<IActionResult> DeleteFamily(long id)
        => HandleResult(await _mediator.Send(new DeleteFamilyCommand(id)));

    [HttpPost("{id:long}/restore")]
    [AllowAnonymous] // TODO: [HasPermission("family.update")]
    public async Task<IActionResult> RestoreFamily(long id)
        => HandleResult(await _mediator.Send(new RestoreFamilyCommand(id)));

    // ==================== WORKFLOW ====================

    [HttpPost("{id:long}/link-student")]
    [AllowAnonymous] // TODO: [HasPermission("family.update")]
    public async Task<IActionResult> LinkStudent(long id, [FromBody] LinkStudentToFamilyRequest request)
    {
        if (id != request.FamilyId) return BadRequest("Id mismatch.");
        return HandleResult(await _mediator.Send(new LinkStudentToFamilyCommand(request)));
    }

    [HttpPost("{id:long}/unlink-student")]
    [AllowAnonymous] // TODO: [HasPermission("family.update")]
    public async Task<IActionResult> UnlinkStudent(long id, [FromBody] UnlinkStudentFromFamilyRequest request)
    {
        if (id != request.FamilyId) return BadRequest("Id mismatch.");
        return HandleResult(await _mediator.Send(new UnlinkStudentFromFamilyCommand(request)));
    }
}
