using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.Application.Common.DTOs;
using SchoolERP.Application.Features.Staff.Commands.Department.BulkDeleteDepartment;
using SchoolERP.Application.Features.Staff.Commands.Department.BulkPatchDepartment;
using SchoolERP.Application.Features.Staff.Commands.Department.BulkUpdateDepartment;
using SchoolERP.Application.Features.Staff.Commands.Department.CreateDepartment;
using SchoolERP.Application.Features.Staff.Commands.Department.DeleteDepartment;
using SchoolERP.Application.Features.Staff.Commands.Department.PatchDepartment;
using SchoolERP.Application.Features.Staff.Commands.Department.RestoreDepartment;
using SchoolERP.Application.Features.Staff.Commands.Department.UpdateDepartment;
using SchoolERP.Application.Features.Staff.DTOs;
using SchoolERP.Application.Features.Staff.Queries.Department.ExportDepartments;
using SchoolERP.Application.Features.Staff.Queries.Department.GetDepartmentById;
using SchoolERP.Application.Features.Staff.Queries.Department.GetDepartments;
using SchoolERP.Application.Features.Staff.Queries.Department.GetDepartmentsLight;

namespace SchoolERP.API.Controllers;

[ApiController]
[Route("api/departments")]
public class DepartmentsController : BaseApiController
{
    private readonly IMediator _mediator;

    public DepartmentsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [AllowAnonymous] // TODO: [HasPermission("department.read")]
    public async Task<IActionResult> GetDepartments([FromQuery] PagedRequest request)
        => HandleResult(await _mediator.Send(new GetDepartmentsQuery(request)));

    [HttpGet("{id:long}")]
    [AllowAnonymous] // TODO: [HasPermission("department.read")]
    public async Task<IActionResult> GetDepartmentById(long id)
        => HandleResult(await _mediator.Send(new GetDepartmentByIdQuery(id)));

    [HttpGet("all")]
    [AllowAnonymous] // TODO: [HasPermission("department.read")]
    public async Task<IActionResult> GetDepartmentsLight()
        => HandleResult(await _mediator.Send(new GetDepartmentsLightQuery()));

    [HttpGet("export")]
    [AllowAnonymous] // TODO: [HasPermission("department.export")]
    public async Task<IActionResult> ExportDepartments()
    {
        var result = await _mediator.Send(new ExportDepartmentsQuery());
        if (result.IsFailure) return HandleResult(result);
        return File(result.Value, "text/csv", "departments.csv");
    }

    [HttpPost]
    [AllowAnonymous] // TODO: [HasPermission("department.create")]
    public async Task<IActionResult> CreateDepartment([FromBody] CreateDepartmentRequest request)
        => HandleResult(await _mediator.Send(new CreateDepartmentCommand(request)));

    [HttpPut("{id:long}")]
    [AllowAnonymous] // TODO: [HasPermission("department.update")]
    public async Task<IActionResult> UpdateDepartment(long id, [FromBody] UpdateDepartmentRequest request)
    {
        if (id != request.Id) return BadRequest("Id mismatch.");
        return HandleResult(await _mediator.Send(new UpdateDepartmentCommand(request)));
    }

    [HttpPatch("{id:long}")]
    [AllowAnonymous] // TODO: [HasPermission("department.update")]
    public async Task<IActionResult> PatchDepartment(long id, [FromBody] PatchDepartmentRequest request)
    {
        if (id != request.Id) return BadRequest("Id mismatch.");
        return HandleResult(await _mediator.Send(new PatchDepartmentCommand(request)));
    }

    [HttpDelete("{id:long}")]
    [AllowAnonymous] // TODO: [HasPermission("department.delete")]
    public async Task<IActionResult> DeleteDepartment(long id)
        => HandleResult(await _mediator.Send(new DeleteDepartmentCommand(id)));

    [HttpPost("{id:long}/restore")]
    [AllowAnonymous] // TODO: [HasPermission("department.update")]
    public async Task<IActionResult> RestoreDepartment(long id)
        => HandleResult(await _mediator.Send(new RestoreDepartmentCommand(id)));

    [HttpPut("bulk")]
    [AllowAnonymous] // TODO: [HasPermission("department.update")]
    public async Task<IActionResult> BulkUpdateDepartment([FromBody] BulkUpdateDepartmentRequest request)
        => HandleResult(await _mediator.Send(new BulkUpdateDepartmentCommand(request)));

    [HttpPatch("bulk")]
    [AllowAnonymous] // TODO: [HasPermission("department.update")]
    public async Task<IActionResult> BulkPatchDepartment([FromBody] BulkPatchDepartmentRequest request)
        => HandleResult(await _mediator.Send(new BulkPatchDepartmentCommand(request)));

    [HttpDelete("bulk")]
    [AllowAnonymous] // TODO: [HasPermission("department.delete")]
    public async Task<IActionResult> BulkDeleteDepartment([FromBody] BulkDeleteDepartmentRequest request)
        => HandleResult(await _mediator.Send(new BulkDeleteDepartmentCommand(request)));
}