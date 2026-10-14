using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.Application.Features.Student.Commands.Enquiry.AssignEnquiry;
using SchoolERP.Application.Features.Student.Commands.Enquiry.ChangeEnquiryStatus;
using SchoolERP.Application.Features.Student.Commands.Enquiry.ConvertEnquiryToRegistration;
using SchoolERP.Application.Features.Student.Commands.Enquiry.CreateEnquiry;
using SchoolERP.Application.Features.Student.Commands.Enquiry.DeleteEnquiry;
using SchoolERP.Application.Features.Student.Commands.Enquiry.PatchEnquiry;
using SchoolERP.Application.Features.Student.Commands.Enquiry.RestoreEnquiry;
using SchoolERP.Application.Features.Student.Commands.Enquiry.UpdateEnquiry;
using SchoolERP.Application.Features.Student.DTOs;
using SchoolERP.Application.Features.Student.Queries.Enquiry.ExportEnquiries;
using SchoolERP.Application.Features.Student.Queries.Enquiry.GetEnquiries;
using SchoolERP.Application.Features.Student.Queries.Enquiry.GetEnquiriesLight;
using SchoolERP.Application.Features.Student.Queries.Enquiry.GetEnquiryById;

namespace SchoolERP.API.Controllers.Student;

[ApiController]
[Route("api/enquiries")]
public class EnquiriesController : BaseApiController
{
    private readonly IMediator _mediator;

    public EnquiriesController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [AllowAnonymous] // TODO: [HasPermission("enquiry.read")]
    public async Task<IActionResult> GetEnquiries([FromQuery] GetEnquiriesRequest request)
        => HandleResult(await _mediator.Send(new GetEnquiriesQuery(request)));

    [HttpGet("{id:long}")]
    [AllowAnonymous] // TODO: [HasPermission("enquiry.read")]
    public async Task<IActionResult> GetEnquiryById(long id)
        => HandleResult(await _mediator.Send(new GetEnquiryByIdQuery(id)));

    [HttpGet("all")]
    [AllowAnonymous] // TODO: [HasPermission("enquiry.read")]
    public async Task<IActionResult> GetEnquiriesLight()
        => HandleResult(await _mediator.Send(new GetEnquiriesLightQuery()));

    [HttpGet("export")]
    [AllowAnonymous] // TODO: [HasPermission("enquiry.export")]
    public async Task<IActionResult> ExportEnquiries()
    {
        var result = await _mediator.Send(new ExportEnquiriesQuery());
        if (result.IsFailure) return HandleResult(result);
        return File(result.Value, "text/csv", "enquiries.csv");
    }

    [HttpPost]
    [AllowAnonymous] // TODO: [HasPermission("enquiry.create")]
    public async Task<IActionResult> CreateEnquiry([FromBody] CreateEnquiryRequest request)
        => HandleResult(await _mediator.Send(new CreateEnquiryCommand(request)));

    [HttpPut("{id:long}")]
    [AllowAnonymous] // TODO: [HasPermission("enquiry.update")]
    public async Task<IActionResult> UpdateEnquiry(long id, [FromBody] UpdateEnquiryRequest request)
    {
        if (id != request.Id) return BadRequest("Id mismatch.");
        return HandleResult(await _mediator.Send(new UpdateEnquiryCommand(request)));
    }

    [HttpPatch("{id:long}")]
    [AllowAnonymous] // TODO: [HasPermission("enquiry.update")]
    public async Task<IActionResult> PatchEnquiry(long id, [FromBody] PatchEnquiryRequest request)
    {
        if (id != request.Id) return BadRequest("Id mismatch.");
        return HandleResult(await _mediator.Send(new PatchEnquiryCommand(request)));
    }

    [HttpDelete("{id:long}")]
    [AllowAnonymous] // TODO: [HasPermission("enquiry.delete")]
    public async Task<IActionResult> DeleteEnquiry(long id)
        => HandleResult(await _mediator.Send(new DeleteEnquiryCommand(id)));

    [HttpPost("{id:long}/restore")]
    [AllowAnonymous] // TODO: [HasPermission("enquiry.update")]
    public async Task<IActionResult> RestoreEnquiry(long id)
        => HandleResult(await _mediator.Send(new RestoreEnquiryCommand(id)));

    [HttpPost("{id:long}/assign")]
    [AllowAnonymous] // TODO: [HasPermission("enquiry.assign")]
    public async Task<IActionResult> AssignEnquiry(long id, [FromBody] AssignEnquiryRequest request)
    {
        if (id != request.Id) return BadRequest("Id mismatch.");
        return HandleResult(await _mediator.Send(new AssignEnquiryCommand(request)));
    }

    [HttpPost("{id:long}/change-status")]
    [AllowAnonymous] // TODO: [HasPermission("enquiry.update")]
    public async Task<IActionResult> ChangeEnquiryStatus(long id, [FromBody] ChangeEnquiryStatusRequest request)
    {
        if (id != request.Id) return BadRequest("Id mismatch.");
        return HandleResult(await _mediator.Send(new ChangeEnquiryStatusCommand(request)));
    }

    [HttpPost("{id:long}/convert")]
    [AllowAnonymous] // TODO: [HasPermission("enquiry.convert")]
    public async Task<IActionResult> ConvertEnquiryToRegistration(long id, [FromBody] ConvertEnquiryToRegistrationRequest request)
    {
        if (id != request.Id) return BadRequest("Id mismatch.");
        return HandleResult(await _mediator.Send(new ConvertEnquiryToRegistrationCommand(request)));
    }
}
