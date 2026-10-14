using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.Application.Features.Student.Commands.Alumni.ArchiveAlumni;
using SchoolERP.Application.Features.Student.Commands.Alumni.ConvertStudentToAlumni;
using SchoolERP.Application.Features.Student.Commands.Alumni.CreateAlumni;
using SchoolERP.Application.Features.Student.Commands.Alumni.DeleteAlumni;
using SchoolERP.Application.Features.Student.Commands.Alumni.PatchAlumni;
using SchoolERP.Application.Features.Student.Commands.Alumni.RestoreAlumni;
using SchoolERP.Application.Features.Student.Commands.Alumni.UpdateAlumni;
using SchoolERP.Application.Features.Student.Commands.Alumni.UpdateAlumniProfile;
using SchoolERP.Application.Features.Student.DTOs;
using SchoolERP.Application.Features.Student.Queries.Alumni.ExportAlumni;
using SchoolERP.Application.Features.Student.Queries.Alumni.GetAlumni;
using SchoolERP.Application.Features.Student.Queries.Alumni.GetAlumniById;
using SchoolERP.Application.Features.Student.Queries.Alumni.GetAlumniLight;

namespace SchoolERP.API.Controllers.Student;

[ApiController]
[Route("api/alumni")]
public class AlumniController : BaseApiController
{
    private readonly IMediator _mediator;

    public AlumniController(IMediator mediator) => _mediator = mediator;

    // ==================== READ ====================

    [HttpGet]
    [AllowAnonymous] // TODO: [HasPermission("alumni.read")]
    public async Task<IActionResult> GetAlumni([FromQuery] GetAlumniRequest request)
        => HandleResult(await _mediator.Send(new GetAlumniQuery(request)));

    [HttpGet("{id:long}")]
    [AllowAnonymous] // TODO: [HasPermission("alumni.read")]
    public async Task<IActionResult> GetAlumniById(long id)
        => HandleResult(await _mediator.Send(new GetAlumniByIdQuery(id)));

    [HttpGet("all")]
    [AllowAnonymous] // TODO: [HasPermission("alumni.read")]
    public async Task<IActionResult> GetAlumniLight()
        => HandleResult(await _mediator.Send(new GetAlumniLightQuery()));

    [HttpGet("export")]
    [AllowAnonymous] // TODO: [HasPermission("alumni.export")]
    public async Task<IActionResult> ExportAlumni()
    {
        var result = await _mediator.Send(new ExportAlumniQuery());
        if (result.IsFailure) return HandleResult(result);
        return File(
            result.Value,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"alumni_{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx"
        );
    }

    // ==================== CRUD ====================

    [HttpPost]
    [AllowAnonymous] // TODO: [HasPermission("alumni.create")]
    public async Task<IActionResult> CreateAlumni([FromBody] CreateAlumniRequest request)
        => HandleResult(await _mediator.Send(new CreateAlumniCommand(request)));

    [HttpPut("{id:long}")]
    [AllowAnonymous] // TODO: [HasPermission("alumni.update")]
    public async Task<IActionResult> UpdateAlumni(long id, [FromBody] UpdateAlumniRequest request)
    {
        if (id != request.Id) return BadRequest("Id mismatch.");
        return HandleResult(await _mediator.Send(new UpdateAlumniCommand(request)));
    }

    [HttpPatch("{id:long}")]
    [AllowAnonymous] // TODO: [HasPermission("alumni.update")]
    public async Task<IActionResult> PatchAlumni(long id, [FromBody] PatchAlumniRequest request)
    {
        if (id != request.Id) return BadRequest("Id mismatch.");
        return HandleResult(await _mediator.Send(new PatchAlumniCommand(request)));
    }

    [HttpDelete("{id:long}")]
    [AllowAnonymous] // TODO: [HasPermission("alumni.delete")]
    public async Task<IActionResult> DeleteAlumni(long id)
        => HandleResult(await _mediator.Send(new DeleteAlumniCommand(id)));

    [HttpPost("{id:long}/restore")]
    [AllowAnonymous] // TODO: [HasPermission("alumni.update")]
    public async Task<IActionResult> RestoreAlumni(long id)
        => HandleResult(await _mediator.Send(new RestoreAlumniCommand(id)));

    // ==================== WORKFLOW ====================

    [HttpPost("{id:long}/update-profile")]
    [AllowAnonymous] // TODO: [HasPermission("alumni.update")]
    public async Task<IActionResult> UpdateAlumniProfile(long id, [FromBody] UpdateAlumniProfileRequest request)
    {
        if (id != request.Id) return BadRequest("Id mismatch.");
        return HandleResult(await _mediator.Send(new UpdateAlumniProfileCommand(request)));
    }

    [HttpPost("convert-from-student")]
    [AllowAnonymous] // TODO: [HasPermission("alumni.convert")]
    public async Task<IActionResult> ConvertStudentToAlumni([FromBody] ConvertStudentToAlumniRequest request)
        => HandleResult(await _mediator.Send(new ConvertStudentToAlumniCommand(request)));

    [HttpPost("{id:long}/archive")]
    [AllowAnonymous] // TODO: [HasPermission("alumni.update")]
    public async Task<IActionResult> ArchiveAlumni(long id, [FromQuery] bool isDeceased = false)
        => HandleResult(await _mediator.Send(new ArchiveAlumniCommand(id, isDeceased)));
}
