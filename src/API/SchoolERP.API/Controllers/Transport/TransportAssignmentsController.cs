using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.Application.Features.Transport.Commands.Assignment.BulkImportTransportAssignmentsCsv;
using SchoolERP.Application.Features.Transport.Commands.Assignment.ChangeTransportAssignment;
using SchoolERP.Application.Features.Transport.Commands.Assignment.CreateTransportAssignment;
using SchoolERP.Application.Features.Transport.Commands.Assignment.EndTransportAssignment;
using SchoolERP.Application.Features.Transport.DTOs.Assignment;
using SchoolERP.Application.Features.Transport.Queries.Assignment.GetStudentTransportAssignment;
using SchoolERP.Application.Features.Transport.Queries.Assignment.GetStudentTransportFeeQuote;
using SchoolERP.Application.Features.Transport.Queries.Assignment.GetStudentTransportHistory;
using SchoolERP.Application.Features.Transport.Queries.Assignment.GetTransportAssignments;

namespace SchoolERP.API.Controllers.Transport;

/// <summary>
/// TEMPORARY: [AllowAnonymous]. When RBAC ready, replace with
/// [HasPermission(PermissionNames.TransportAssignmentXxx)].
/// </summary>
[ApiController]
[Route("api/transport/assignments")]
[AllowAnonymous]
public sealed class TransportAssignmentsController : BaseApiController
{
    private readonly IMediator _mediator;
    public TransportAssignmentsController(IMediator mediator) => _mediator = mediator;

    // ----- Reads -----

    [HttpGet]
    [AllowAnonymous]  // TODO: [HasPermission(PermissionNames.TransportAssignmentRead)]
    public async Task<IActionResult> Get([FromQuery] GetTransportAssignmentsQuery query, CancellationToken ct)
        => HandleResult(await _mediator.Send(query, ct));

    [HttpGet("students/{studentId:long}")]
    [AllowAnonymous]  // TODO: [HasPermission(PermissionNames.TransportAssignmentRead)]
    public async Task<IActionResult> GetStudent([FromRoute] long studentId, [FromQuery] long? academicYearId, CancellationToken ct)
        => HandleResult(await _mediator.Send(new GetStudentTransportAssignmentQuery(studentId, academicYearId), ct));

    [HttpGet("students/{studentId:long}/history")]
    [AllowAnonymous]  // TODO: [HasPermission(PermissionNames.TransportAssignmentRead)]
    public async Task<IActionResult> GetHistory([FromRoute] long studentId, CancellationToken ct)
        => HandleResult(await _mediator.Send(new GetStudentTransportHistoryQuery(studentId), ct));

    [HttpGet("students/{studentId:long}/fee-quote")]
    [AllowAnonymous]  // TODO: [HasPermission(PermissionNames.TransportAssignmentRead)]
    public async Task<IActionResult> GetFeeQuote([FromRoute] long studentId, [FromQuery] long academicYearId, CancellationToken ct)
        => HandleResult(await _mediator.Send(new GetStudentTransportFeeQuoteQuery(studentId, academicYearId), ct));

    // ----- Writes -----

    [HttpPost]
    [AllowAnonymous]  // TODO: [HasPermission(PermissionNames.TransportAssignmentCreate)]
    public async Task<IActionResult> Create([FromBody] CreateTransportAssignmentRequest request, CancellationToken ct)
        => HandleResult(await _mediator.Send(new CreateTransportAssignmentCommand(request), ct));

    [HttpPost("change")]
    [AllowAnonymous]  // TODO: [HasPermission(PermissionNames.TransportAssignmentChange)]
    public async Task<IActionResult> Change([FromBody] ChangeTransportAssignmentRequest request, CancellationToken ct)
        => HandleResult(await _mediator.Send(new ChangeTransportAssignmentCommand(request), ct));

    [HttpPost("{id:long}/end")]
    [AllowAnonymous]  // TODO: [HasPermission(PermissionNames.TransportAssignmentEnd)]
    public async Task<IActionResult> End([FromRoute] long id, [FromBody] EndTransportAssignmentRequest request, CancellationToken ct)
        => HandleResult(await _mediator.Send(new EndTransportAssignmentCommand(id, request), ct));

    [HttpPost("bulk-csv")]
    [AllowAnonymous]  // TODO: [HasPermission(PermissionNames.TransportAssignmentBulkCsv)]
    public async Task<IActionResult> BulkCsv([FromBody] BulkTransportAssignmentCsvRequest request, CancellationToken ct)
        => HandleResult(await _mediator.Send(new BulkImportTransportAssignmentsCsvCommand(request), ct));
}
