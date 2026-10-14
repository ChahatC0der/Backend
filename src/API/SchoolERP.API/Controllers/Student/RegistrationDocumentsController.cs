using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.Application.Common.DTOs;
using SchoolERP.Application.Features.Student.Commands.RegistrationDocument.CreateRegistrationDocument;
using SchoolERP.Application.Features.Student.Commands.RegistrationDocument.DeleteRegistrationDocument;
using SchoolERP.Application.Features.Student.Commands.RegistrationDocument.PatchRegistrationDocument;
using SchoolERP.Application.Features.Student.Commands.RegistrationDocument.RestoreRegistrationDocument;
using SchoolERP.Application.Features.Student.Commands.RegistrationDocument.UpdateRegistrationDocument;
using SchoolERP.Application.Features.Student.Commands.RegistrationDocument.VerifyRegistrationDocument;
using SchoolERP.Application.Features.Student.DTOs;
using SchoolERP.Application.Features.Student.Queries.RegistrationDocument.ExportRegistrationDocuments;
using SchoolERP.Application.Features.Student.Queries.RegistrationDocument.GetRegistrationDocumentById;
using SchoolERP.Application.Features.Student.Queries.RegistrationDocument.GetRegistrationDocuments;
using SchoolERP.Application.Features.Student.Queries.RegistrationDocument.GetRegistrationDocumentsLight;

namespace SchoolERP.API.Controllers.Student;

[ApiController]
[Route("api/registration-documents")]
public class RegistrationDocumentsController : BaseApiController
{
    private readonly IMediator _mediator;

    public RegistrationDocumentsController(IMediator mediator) => _mediator = mediator;

    // ==================== READ ====================

    [HttpGet]
    [AllowAnonymous] // TODO: [HasPermission("registration.read")]
    public async Task<IActionResult> GetRegistrationDocuments([FromQuery] PagedRequest request, [FromQuery] long? registrationId = null)
        => HandleResult(await _mediator.Send(new GetRegistrationDocumentsQuery(request, registrationId)));

    [HttpGet("{id:long}")]
    [AllowAnonymous] // TODO: [HasPermission("registration.read")]
    public async Task<IActionResult> GetRegistrationDocumentById(long id)
        => HandleResult(await _mediator.Send(new GetRegistrationDocumentByIdQuery(id)));

    [HttpGet("by-registration/{registrationId:long}")]
    [AllowAnonymous] // TODO: [HasPermission("registration.read")]
    public async Task<IActionResult> GetRegistrationDocumentsLight(long registrationId)
        => HandleResult(await _mediator.Send(new GetRegistrationDocumentsLightQuery(registrationId)));

    [HttpGet("export/{registrationId:long}")]
    [AllowAnonymous] // TODO: [HasPermission("registration.export")]
    public async Task<IActionResult> ExportRegistrationDocuments(long registrationId)
    {
        var result = await _mediator.Send(new ExportRegistrationDocumentsQuery(registrationId));
        if (result.IsFailure) return HandleResult(result);
        return File(
            result.Value,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"registration_documents_{registrationId}_{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx"
        );
    }

    // ==================== CRUD ====================

    [HttpPost]
    [AllowAnonymous] // TODO: [HasPermission("registration.update")]
    public async Task<IActionResult> CreateRegistrationDocument([FromBody] CreateRegistrationDocumentRequest request)
        => HandleResult(await _mediator.Send(new CreateRegistrationDocumentCommand(request)));

    [HttpPut("{id:long}")]
    [AllowAnonymous] // TODO: [HasPermission("registration.update")]
    public async Task<IActionResult> UpdateRegistrationDocument(long id, [FromBody] UpdateRegistrationDocumentRequest request)
    {
        if (id != request.Id) return BadRequest("Id mismatch.");
        return HandleResult(await _mediator.Send(new UpdateRegistrationDocumentCommand(request)));
    }

    [HttpPatch("{id:long}")]
    [AllowAnonymous] // TODO: [HasPermission("registration.update")]
    public async Task<IActionResult> PatchRegistrationDocument(long id, [FromBody] PatchRegistrationDocumentRequest request)
    {
        if (id != request.Id) return BadRequest("Id mismatch.");
        return HandleResult(await _mediator.Send(new PatchRegistrationDocumentCommand(request)));
    }

    [HttpDelete("{id:long}")]
    [AllowAnonymous] // TODO: [HasPermission("registration.update")]
    public async Task<IActionResult> DeleteRegistrationDocument(long id)
        => HandleResult(await _mediator.Send(new DeleteRegistrationDocumentCommand(id)));

    [HttpPost("{id:long}/restore")]
    [AllowAnonymous] // TODO: [HasPermission("registration.update")]
    public async Task<IActionResult> RestoreRegistrationDocument(long id)
        => HandleResult(await _mediator.Send(new RestoreRegistrationDocumentCommand(id)));

    // ==================== WORKFLOW ====================

    [HttpPost("{id:long}/verify")]
    [AllowAnonymous] // TODO: [HasPermission("registration.verify")]
    public async Task<IActionResult> VerifyRegistrationDocument(long id, [FromBody] VerifyRegistrationDocumentRequest request)
    {
        if (id != request.Id) return BadRequest("Id mismatch.");
        return HandleResult(await _mediator.Send(new VerifyRegistrationDocumentCommand(request)));
    }
}
