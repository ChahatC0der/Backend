using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.Application.Features.Student.Commands.Registration.ApproveRegistration;
using SchoolERP.Application.Features.Student.Commands.Registration.ConvertRegistrationToAdmission;
using SchoolERP.Application.Features.Student.Commands.Registration.CreateRegistration;
using SchoolERP.Application.Features.Student.Commands.Registration.DeleteRegistration;
using SchoolERP.Application.Features.Student.Commands.Registration.PatchRegistration;
using SchoolERP.Application.Features.Student.Commands.Registration.RecordInterviewResult;
using SchoolERP.Application.Features.Student.Commands.Registration.RecordRegistrationFee;
using SchoolERP.Application.Features.Student.Commands.Registration.RejectRegistration;
using SchoolERP.Application.Features.Student.Commands.Registration.RestoreRegistration;
using SchoolERP.Application.Features.Student.Commands.Registration.ScheduleInterview;
using SchoolERP.Application.Features.Student.Commands.Registration.StartRegistrationReview;
using SchoolERP.Application.Features.Student.Commands.Registration.SubmitRegistration;
using SchoolERP.Application.Features.Student.Commands.Registration.UpdateRegistration;
using SchoolERP.Application.Features.Student.DTOs;
using SchoolERP.Application.Features.Student.Queries.Registration.ExportRegistrations;
using SchoolERP.Application.Features.Student.Queries.Registration.GetRegistrationById;
using SchoolERP.Application.Features.Student.Queries.Registration.GetRegistrations;
using SchoolERP.Application.Features.Student.Queries.Registration.GetRegistrationsLight;

namespace SchoolERP.API.Controllers.Student;

[ApiController]
[Route("api/registrations")]
public class RegistrationsController : BaseApiController
{
    private readonly IMediator _mediator;

    public RegistrationsController(IMediator mediator) => _mediator = mediator;

    // ==================== READ ====================

    [HttpGet]
    [AllowAnonymous] // TODO: [HasPermission("registration.read")]
    public async Task<IActionResult> GetRegistrations([FromQuery] GetRegistrationsRequest request)
        => HandleResult(await _mediator.Send(new GetRegistrationsQuery(request)));

    [HttpGet("{id:long}")]
    [AllowAnonymous] // TODO: [HasPermission("registration.read")]
    public async Task<IActionResult> GetRegistrationById(long id)
        => HandleResult(await _mediator.Send(new GetRegistrationByIdQuery(id)));

    [HttpGet("all")]
    [AllowAnonymous] // TODO: [HasPermission("registration.read")]
    public async Task<IActionResult> GetRegistrationsLight()
        => HandleResult(await _mediator.Send(new GetRegistrationsLightQuery()));

    [HttpGet("export")]
    [AllowAnonymous] // TODO: [HasPermission("registration.export")]
    public async Task<IActionResult> ExportRegistrations()
    {
        var result = await _mediator.Send(new ExportRegistrationsQuery());
        if (result.IsFailure) return HandleResult(result);
        return File(
            result.Value,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"registrations_{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx"
        );
    }

    // ==================== CRUD ====================

    [HttpPost]
    [AllowAnonymous] // TODO: [HasPermission("registration.create")]
    public async Task<IActionResult> CreateRegistration([FromBody] CreateRegistrationRequest request)
        => HandleResult(await _mediator.Send(new CreateRegistrationCommand(request)));

    [HttpPut("{id:long}")]
    [AllowAnonymous] // TODO: [HasPermission("registration.update")]
    public async Task<IActionResult> UpdateRegistration(long id, [FromBody] UpdateRegistrationRequest request)
    {
        if (id != request.Id) return BadRequest("Id mismatch.");
        return HandleResult(await _mediator.Send(new UpdateRegistrationCommand(request)));
    }

    [HttpPatch("{id:long}")]
    [AllowAnonymous] // TODO: [HasPermission("registration.update")]
    public async Task<IActionResult> PatchRegistration(long id, [FromBody] PatchRegistrationRequest request)
    {
        if (id != request.Id) return BadRequest("Id mismatch.");
        return HandleResult(await _mediator.Send(new PatchRegistrationCommand(request)));
    }

    [HttpDelete("{id:long}")]
    [AllowAnonymous] // TODO: [HasPermission("registration.delete")]
    public async Task<IActionResult> DeleteRegistration(long id)
        => HandleResult(await _mediator.Send(new DeleteRegistrationCommand(id)));

    [HttpPost("{id:long}/restore")]
    [AllowAnonymous] // TODO: [HasPermission("registration.update")]
    public async Task<IActionResult> RestoreRegistration(long id)
        => HandleResult(await _mediator.Send(new RestoreRegistrationCommand(id)));

    // ==================== WORKFLOW ====================

    [HttpPost("{id:long}/submit")]
    [AllowAnonymous] // TODO: [HasPermission("registration.update")]
    public async Task<IActionResult> SubmitRegistration(long id, [FromBody] SubmitRegistrationRequest request)
    {
        if (id != request.Id) return BadRequest("Id mismatch.");
        return HandleResult(await _mediator.Send(new SubmitRegistrationCommand(request)));
    }

    [HttpPost("{id:long}/start-review")]
    [AllowAnonymous] // TODO: [HasPermission("registration.verify")]
    public async Task<IActionResult> StartRegistrationReview(long id, [FromBody] StartRegistrationReviewRequest request)
    {
        if (id != request.Id) return BadRequest("Id mismatch.");
        return HandleResult(await _mediator.Send(new StartRegistrationReviewCommand(request)));
    }

    [HttpPost("{id:long}/schedule-interview")]
    [AllowAnonymous] // TODO: [HasPermission("registration.verify")]
    public async Task<IActionResult> ScheduleInterview(long id, [FromBody] ScheduleInterviewRequest request)
    {
        if (id != request.Id) return BadRequest("Id mismatch.");
        return HandleResult(await _mediator.Send(new ScheduleInterviewCommand(request)));
    }

    [HttpPost("{id:long}/interview-result")]
    [AllowAnonymous] // TODO: [HasPermission("registration.verify")]
    public async Task<IActionResult> RecordInterviewResult(long id, [FromBody] RecordInterviewResultRequest request)
    {
        if (id != request.Id) return BadRequest("Id mismatch.");
        return HandleResult(await _mediator.Send(new RecordInterviewResultCommand(request)));
    }

    [HttpPost("{id:long}/approve")]
    [AllowAnonymous] // TODO: [HasPermission("registration.approve")]
    public async Task<IActionResult> ApproveRegistration(long id, [FromBody] ApproveRegistrationRequest request)
    {
        if (id != request.Id) return BadRequest("Id mismatch.");
        return HandleResult(await _mediator.Send(new ApproveRegistrationCommand(request)));
    }

    [HttpPost("{id:long}/reject")]
    [AllowAnonymous] // TODO: [HasPermission("registration.approve")]
    public async Task<IActionResult> RejectRegistration(long id, [FromBody] RejectRegistrationRequest request)
    {
        if (id != request.Id) return BadRequest("Id mismatch.");
        return HandleResult(await _mediator.Send(new RejectRegistrationCommand(request)));
    }

    [HttpPost("{id:long}/record-fee")]
    [AllowAnonymous] // TODO: [HasPermission("registration.update")]
    public async Task<IActionResult> RecordRegistrationFee(long id, [FromBody] RecordRegistrationFeeRequest request)
    {
        if (id != request.Id) return BadRequest("Id mismatch.");
        return HandleResult(await _mediator.Send(new RecordRegistrationFeeCommand(request)));
    }

    [HttpPost("{id:long}/convert-to-admission")]
    [AllowAnonymous] // TODO: [HasPermission("registration.convert")]
    public async Task<IActionResult> ConvertRegistrationToAdmission(long id, [FromBody] ConvertRegistrationToAdmissionRequest request)
    {
        if (id != request.Id) return BadRequest("Id mismatch.");
        return HandleResult(await _mediator.Send(new ConvertRegistrationToAdmissionCommand(request)));
    }
}
