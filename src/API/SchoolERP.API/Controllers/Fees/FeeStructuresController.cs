using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.API.Authorization;
using SchoolERP.Application.Features.Fees.Commands.FeeStructure.AssignStudentFeeStructure;
using SchoolERP.Application.Features.Fees.Commands.FeeStructure.CloneFeeStructure;
using SchoolERP.Application.Features.Fees.Commands.FeeStructure.CreateFeeStructure;
using SchoolERP.Application.Features.Fees.Commands.FeeStructure.DeleteFeeStructure;
using SchoolERP.Application.Features.Fees.Commands.FeeStructure.PatchFeeStructureStatus;
using SchoolERP.Application.Features.Fees.Commands.FeeStructure.UpdateFeeStructure;
using SchoolERP.Application.Features.Fees.DTOs.FeeStructure;
using SchoolERP.Application.Features.Fees.Queries.FeeStructure.GetFeeStructureById;
using SchoolERP.Application.Features.Fees.Queries.FeeStructure.GetFeeStructures;
using SchoolERP.Application.Features.Fees.Queries.FeeStructure.GetFeeStructuresLight;
using SchoolERP.Application.Features.Fees.Queries.FeeStructure.GetStudentAssignedStructure;
using SchoolERP.Domain.Shared.Permissions;

namespace SchoolERP.API.Controllers.Fees;

[ApiController]
[Route("api/fees/structures")]
public sealed class FeeStructuresController : BaseApiController
{
    private readonly IMediator _mediator;
    public FeeStructuresController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [AllowAnonymous] //[HasPermission(PermissionNames.FeeStructureRead)]
    public async Task<IActionResult> Get([FromQuery] GetFeeStructuresQuery query, CancellationToken ct)
        => HandleResult(await _mediator.Send(query, ct));

    [HttpGet("light")]
    [AllowAnonymous] //[HasPermission(PermissionNames.FeeStructureRead)]
    public async Task<IActionResult> GetLight([FromQuery] GetFeeStructuresLightQuery query, CancellationToken ct)
        => HandleResult(await _mediator.Send(query, ct));

    [HttpGet("{id:long}")]
    [AllowAnonymous] //[HasPermission(PermissionNames.FeeStructureRead)]
    public async Task<IActionResult> GetById([FromRoute] long id, CancellationToken ct)
        => HandleResult(await _mediator.Send(new GetFeeStructureByIdQuery(id), ct));

    [HttpGet("student/{studentId:long}")]
    [AllowAnonymous] //[HasPermission(PermissionNames.FeeStructureRead)]
    public async Task<IActionResult> GetStudentAssigned([FromRoute] long studentId, [FromQuery] long academicYearId, CancellationToken ct)
        => HandleResult(await _mediator.Send(new GetStudentAssignedStructureQuery(studentId, academicYearId), ct));

    [HttpPost]
    [AllowAnonymous] //[HasPermission(PermissionNames.FeeStructureCreate)]
    public async Task<IActionResult> Create([FromBody] CreateFeeStructureRequest request, CancellationToken ct)
        => HandleResult(await _mediator.Send(new CreateFeeStructureCommand(request), ct));

    [HttpPut("{id:long}")]
    [AllowAnonymous] //[HasPermission(PermissionNames.FeeStructureUpdate)]
    public async Task<IActionResult> Update([FromRoute] long id, [FromBody] UpdateFeeStructureRequest request, CancellationToken ct)
        => HandleResult(await _mediator.Send(new UpdateFeeStructureCommand(id, request), ct));

    [HttpPost("{id:long}/clone")]
    [AllowAnonymous] //[HasPermission(PermissionNames.FeeStructureClone)]
    public async Task<IActionResult> Clone([FromRoute] long id, [FromBody] CloneFeeStructureRequest request, CancellationToken ct)
        => HandleResult(await _mediator.Send(new CloneFeeStructureCommand(id, request), ct));

    [HttpPatch("{id:long}/status")]
    [AllowAnonymous] //[HasPermission(PermissionNames.FeeStructureUpdate)]
    public async Task<IActionResult> PatchStatus([FromRoute] long id, [FromBody] PatchFeeStructureStatusRequest request, CancellationToken ct)
        => HandleResult(await _mediator.Send(new PatchFeeStructureStatusCommand(id, request), ct));

    [HttpPost("assign-student")]
    [AllowAnonymous] //[HasPermission(PermissionNames.FeeStructureAssign)]
    public async Task<IActionResult> AssignStudent([FromBody] AssignStudentFeeStructureRequest request, CancellationToken ct)
        => HandleResult(await _mediator.Send(new AssignStudentFeeStructureCommand(request), ct));

    [HttpDelete("{id:long}")]
    [AllowAnonymous] //[HasPermission(PermissionNames.FeeStructureDelete)]
    public async Task<IActionResult> Delete([FromRoute] long id, CancellationToken ct)
        => HandleResult(await _mediator.Send(new DeleteFeeStructureCommand(id), ct));
}
