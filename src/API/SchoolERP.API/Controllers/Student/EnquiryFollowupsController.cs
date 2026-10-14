using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.Application.Features.Student.Commands.EnquiryFollowup.CreateEnquiryFollowup;
using SchoolERP.Application.Features.Student.Commands.EnquiryFollowup.DeleteEnquiryFollowup;
using SchoolERP.Application.Features.Student.Commands.EnquiryFollowup.PatchEnquiryFollowup;
using SchoolERP.Application.Features.Student.Commands.EnquiryFollowup.RestoreEnquiryFollowup;
using SchoolERP.Application.Features.Student.Commands.EnquiryFollowup.UpdateEnquiryFollowup;
using SchoolERP.Application.Features.Student.DTOs;
using SchoolERP.Application.Features.Student.Queries.EnquiryFollowup.GetEnquiryFollowupById;
using SchoolERP.Application.Features.Student.Queries.EnquiryFollowup.GetEnquiryFollowups;

namespace SchoolERP.API.Controllers.Student;

[ApiController]
[Route("api/enquiry-followups")]
public class EnquiryFollowupsController : BaseApiController
{
    private readonly IMediator _mediator;

    public EnquiryFollowupsController(IMediator mediator) => _mediator = mediator;

    [HttpGet("by-enquiry/{enquiryId:long}")]
    [AllowAnonymous] // TODO: [HasPermission("enquiry.read")]
    public async Task<IActionResult> GetByEnquiry(long enquiryId)
        => HandleResult(await _mediator.Send(new GetEnquiryFollowupsQuery(enquiryId)));

    [HttpGet("{id:long}")]
    [AllowAnonymous] // TODO: [HasPermission("enquiry.read")]
    public async Task<IActionResult> GetById(long id)
        => HandleResult(await _mediator.Send(new GetEnquiryFollowupByIdQuery(id)));

    [HttpPost]
    [AllowAnonymous] // TODO: [HasPermission("enquiry.update")]
    public async Task<IActionResult> Create([FromBody] CreateEnquiryFollowupRequest request)
        => HandleResult(await _mediator.Send(new CreateEnquiryFollowupCommand(request)));

    [HttpPut("{id:long}")]
    [AllowAnonymous] // TODO: [HasPermission("enquiry.update")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateEnquiryFollowupRequest request)
    {
        if (id != request.Id) return BadRequest("Id mismatch.");
        return HandleResult(await _mediator.Send(new UpdateEnquiryFollowupCommand(request)));
    }

    [HttpPatch("{id:long}")]
    [AllowAnonymous] // TODO: [HasPermission("enquiry.update")]
    public async Task<IActionResult> Patch(long id, [FromBody] PatchEnquiryFollowupRequest request)
    {
        if (id != request.Id) return BadRequest("Id mismatch.");
        return HandleResult(await _mediator.Send(new PatchEnquiryFollowupCommand(request)));
    }

    [HttpDelete("{id:long}")]
    [AllowAnonymous] // TODO: [HasPermission("enquiry.update")]
    public async Task<IActionResult> Delete(long id)
        => HandleResult(await _mediator.Send(new DeleteEnquiryFollowupCommand(id)));

    [HttpPost("{id:long}/restore")]
    [AllowAnonymous] // TODO: [HasPermission("enquiry.update")]
    public async Task<IActionResult> Restore(long id)
        => HandleResult(await _mediator.Send(new RestoreEnquiryFollowupCommand(id)));
}
