using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.Application.Common.DTOs;
using SchoolERP.Application.Features.Staff.Commands.StaffDocument.BulkDeleteStaffDocument;
using SchoolERP.Application.Features.Staff.Commands.StaffDocument.BulkPatchStaffDocument;
using SchoolERP.Application.Features.Staff.Commands.StaffDocument.BulkUpdateStaffDocument;
using SchoolERP.Application.Features.Staff.Commands.StaffDocument.CreateStaffDocument;
using SchoolERP.Application.Features.Staff.Commands.StaffDocument.DeleteStaffDocument;
using SchoolERP.Application.Features.Staff.Commands.StaffDocument.PatchStaffDocument;
using SchoolERP.Application.Features.Staff.Commands.StaffDocument.RestoreStaffDocument;
using SchoolERP.Application.Features.Staff.Commands.StaffDocument.UpdateStaffDocument;
using SchoolERP.Application.Features.Staff.DTOs;
using SchoolERP.Application.Features.Staff.Queries.StaffDocument.ExportStaffDocuments;
using SchoolERP.Application.Features.Staff.Queries.StaffDocument.GetStaffDocumentById;
using SchoolERP.Application.Features.Staff.Queries.StaffDocument.GetStaffDocuments;
using SchoolERP.Application.Features.Staff.Queries.StaffDocument.GetStaffDocumentsLight;

namespace SchoolERP.API.Controllers;

[ApiController]
[Route("api/staff-documents")]
public class StaffDocumentsController : BaseApiController
{
    private readonly IMediator _mediator;

    public StaffDocumentsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [AllowAnonymous] // TODO: [HasPermission("staff_document.read")]
    public async Task<IActionResult> GetStaffDocuments([FromQuery] PagedRequest request, [FromQuery] long? staffId = null)
        => HandleResult(await _mediator.Send(new GetStaffDocumentsQuery(request, staffId)));

    [HttpGet("{id:long}")]
    [AllowAnonymous] // TODO: [HasPermission("staff_document.read")]
    public async Task<IActionResult> GetStaffDocumentById(long id)
        => HandleResult(await _mediator.Send(new GetStaffDocumentByIdQuery(id)));

    [HttpGet("by-staff/{staffId:long}")]
    [AllowAnonymous] // TODO: [HasPermission("staff_document.read")]
    public async Task<IActionResult> GetStaffDocumentsLight(long staffId)
        => HandleResult(await _mediator.Send(new GetStaffDocumentsLightQuery(staffId)));

    [HttpGet("export/{staffId:long}")]
    [AllowAnonymous] // TODO: [HasPermission("staff_document.export")]
    public async Task<IActionResult> ExportStaffDocuments(long staffId)
    {
        var result = await _mediator.Send(new ExportStaffDocumentsQuery(staffId));
        if (result.IsFailure) return HandleResult(result);
        return File(result.Value, "text/csv", "staff_documents.csv");
    }

    [HttpPost]
    [AllowAnonymous] // TODO: [HasPermission("staff_document.create")]
    public async Task<IActionResult> CreateStaffDocument([FromBody] CreateStaffDocumentRequest request)
        => HandleResult(await _mediator.Send(new CreateStaffDocumentCommand(request)));

    [HttpPut("{id:long}")]
    [AllowAnonymous] // TODO: [HasPermission("staff_document.update")]
    public async Task<IActionResult> UpdateStaffDocument(long id, [FromBody] UpdateStaffDocumentRequest request)
    {
        if (id != request.Id) return BadRequest("Id mismatch.");
        return HandleResult(await _mediator.Send(new UpdateStaffDocumentCommand(request)));
    }

    [HttpPatch("{id:long}")]
    [AllowAnonymous] // TODO: [HasPermission("staff_document.update")]
    public async Task<IActionResult> PatchStaffDocument(long id, [FromBody] PatchStaffDocumentRequest request)
    {
        if (id != request.Id) return BadRequest("Id mismatch.");
        return HandleResult(await _mediator.Send(new PatchStaffDocumentCommand(request)));
    }

    [HttpDelete("{id:long}")]
    [AllowAnonymous] // TODO: [HasPermission("staff_document.delete")]
    public async Task<IActionResult> DeleteStaffDocument(long id)
        => HandleResult(await _mediator.Send(new DeleteStaffDocumentCommand(id)));

    [HttpPost("{id:long}/restore")]
    [AllowAnonymous] // TODO: [HasPermission("staff_document.update")]
    public async Task<IActionResult> RestoreStaffDocument(long id)
        => HandleResult(await _mediator.Send(new RestoreStaffDocumentCommand(id)));

    [HttpPut("bulk")]
    [AllowAnonymous] // TODO: [HasPermission("staff_document.update")]
    public async Task<IActionResult> BulkUpdateStaffDocument([FromBody] BulkUpdateStaffDocumentRequest request)
        => HandleResult(await _mediator.Send(new BulkUpdateStaffDocumentCommand(request)));

    [HttpPatch("bulk")]
    [AllowAnonymous] // TODO: [HasPermission("staff_document.update")]
    public async Task<IActionResult> BulkPatchStaffDocument([FromBody] BulkPatchStaffDocumentRequest request)
        => HandleResult(await _mediator.Send(new BulkPatchStaffDocumentCommand(request)));

    [HttpDelete("bulk")]
    [AllowAnonymous] // TODO: [HasPermission("staff_document.delete")]
    public async Task<IActionResult> BulkDeleteStaffDocument([FromBody] BulkDeleteStaffDocumentRequest request)
        => HandleResult(await _mediator.Send(new BulkDeleteStaffDocumentCommand(request)));
}