using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.Application.Common.DTOs;
using SchoolERP.Application.Features.Staff.Commands.Staff.BulkDeleteStaff;
using SchoolERP.Application.Features.Staff.Commands.Staff.BulkPatchStaff;
using SchoolERP.Application.Features.Staff.Commands.Staff.BulkUpdateStaff;
using SchoolERP.Application.Features.Staff.Commands.Staff.CreateStaff;
using SchoolERP.Application.Features.Staff.Commands.Staff.DeleteStaff;
using SchoolERP.Application.Features.Staff.Commands.Staff.PatchStaff;
using SchoolERP.Application.Features.Staff.Commands.Staff.RestoreStaff;
using SchoolERP.Application.Features.Staff.Commands.Staff.UpdateStaff;
using SchoolERP.Application.Features.Staff.DTOs;
using SchoolERP.Application.Features.Staff.Queries.Staff.ExportStaffs;
using SchoolERP.Application.Features.Staff.Queries.Staff.GetStaffById;
using SchoolERP.Application.Features.Staff.Queries.Staff.GetStaffs;
using SchoolERP.Application.Features.Staff.Queries.Staff.GetStaffsLight;

namespace SchoolERP.API.Controllers;

[ApiController]
[Route("api/staff")]
public class StaffController : BaseApiController
{
    private readonly IMediator _mediator;

    public StaffController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [AllowAnonymous] // TODO: [HasPermission("staff.read")]
    public async Task<IActionResult> GetStaffs([FromQuery] PagedRequest request)
        => HandleResult(await _mediator.Send(new GetStaffsQuery(request)));

    [HttpGet("{id:long}")]
    [AllowAnonymous] // TODO: [HasPermission("staff.read")]
    public async Task<IActionResult> GetStaffById(long id)
        => HandleResult(await _mediator.Send(new GetStaffByIdQuery(id)));

    [HttpGet("all")]
    [AllowAnonymous] // TODO: [HasPermission("staff.read")]
    public async Task<IActionResult> GetStaffsLight()
        => HandleResult(await _mediator.Send(new GetStaffsLightQuery()));

    [HttpGet("export")]
    [AllowAnonymous] // TODO: [HasPermission("staff.export")]
    public async Task<IActionResult> ExportStaffs()
    {
        var result = await _mediator.Send(new ExportStaffsQuery());
        if (result.IsFailure) return HandleResult(result);
        return File(result.Value, "text/csv", "staff.csv");
    }

    [HttpPost]
    [AllowAnonymous] // TODO: [HasPermission("staff.create")]
    public async Task<IActionResult> CreateStaff([FromBody] CreateStaffRequest request)
        => HandleResult(await _mediator.Send(new CreateStaffCommand(request)));

    [HttpPut("{id:long}")]
    [AllowAnonymous] // TODO: [HasPermission("staff.update")]
    public async Task<IActionResult> UpdateStaff(long id, [FromBody] UpdateStaffRequest request)
    {
        if (id != request.Id) return BadRequest("Id mismatch.");
        return HandleResult(await _mediator.Send(new UpdateStaffCommand(request)));
    }

    [HttpPatch("{id:long}")]
    [AllowAnonymous] // TODO: [HasPermission("staff.update")]
    public async Task<IActionResult> PatchStaff(long id, [FromBody] PatchStaffRequest request)
    {
        if (id != request.Id) return BadRequest("Id mismatch.");
        return HandleResult(await _mediator.Send(new PatchStaffCommand(request)));
    }

    [HttpDelete("{id:long}")]
    [AllowAnonymous] // TODO: [HasPermission("staff.delete")]
    public async Task<IActionResult> DeleteStaff(long id)
        => HandleResult(await _mediator.Send(new DeleteStaffCommand(id)));

    [HttpPost("{id:long}/restore")]
    [AllowAnonymous] // TODO: [HasPermission("staff.update")]
    public async Task<IActionResult> RestoreStaff(long id)
        => HandleResult(await _mediator.Send(new RestoreStaffCommand(id)));

    [HttpPut("bulk")]
    [AllowAnonymous] // TODO: [HasPermission("staff.update")]
    public async Task<IActionResult> BulkUpdateStaff([FromBody] BulkUpdateStaffRequest request)
        => HandleResult(await _mediator.Send(new BulkUpdateStaffCommand(request)));

    [HttpPatch("bulk")]
    [AllowAnonymous] // TODO: [HasPermission("staff.update")]
    public async Task<IActionResult> BulkPatchStaff([FromBody] BulkPatchStaffRequest request)
        => HandleResult(await _mediator.Send(new BulkPatchStaffCommand(request)));

    [HttpDelete("bulk")]
    [AllowAnonymous] // TODO: [HasPermission("staff.delete")]
    public async Task<IActionResult> BulkDeleteStaff([FromBody] BulkDeleteStaffRequest request)
        => HandleResult(await _mediator.Send(new BulkDeleteStaffCommand(request)));
}