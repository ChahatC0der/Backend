using MediatR;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.API.Authorization;
using SchoolERP.Application.Features.Fees.Commands.Invoice.AddManualInvoiceLine;
using SchoolERP.Application.Features.Fees.Commands.Invoice.AdjustInvoiceAmount;
using SchoolERP.Application.Features.Fees.Commands.Invoice.BulkGenerateInvoices;
using SchoolERP.Application.Features.Fees.Commands.Invoice.CancelInvoice;
using SchoolERP.Application.Features.Fees.Commands.Invoice.GenerateStudentInvoice;
using SchoolERP.Application.Features.Fees.Commands.Invoice.MarkOverdueInvoices;
using SchoolERP.Application.Features.Fees.Commands.Invoice.RecalculateLateFees;
using SchoolERP.Application.Features.Fees.Commands.Invoice.RegenerateInvoice;
using SchoolERP.Application.Features.Fees.Commands.Invoice.WaiveLateFee;
using SchoolERP.Application.Features.Fees.DTOs.Invoice;
using SchoolERP.Application.Features.Fees.Queries.Invoice.GetInvoiceById;
using SchoolERP.Application.Features.Fees.Queries.Invoice.GetInvoices;
using SchoolERP.Application.Features.Fees.Queries.Invoice.GetStudentFeeCounter;
using SchoolERP.Domain.Shared.Permissions;

namespace SchoolERP.API.Controllers.Fees;

[ApiController]
[Route("api/fees/invoices")]
public sealed class InvoicesController : BaseApiController
{
    private readonly IMediator _mediator;
    public InvoicesController(IMediator mediator) => _mediator = mediator;

    // ----- Reads -----

    [HttpGet]
    [HasPermission(PermissionNames.FeeInvoiceRead)]
    public async Task<IActionResult> Get([FromQuery] GetInvoicesQuery query, CancellationToken ct)
        => HandleResult(await _mediator.Send(query, ct));

    [HttpGet("counter/{studentId:long}")]
    [HasPermission(PermissionNames.FeeInvoiceRead)]
    public async Task<IActionResult> GetCounter([FromRoute] long studentId, [FromQuery] long? academicYearId, CancellationToken ct)
        => HandleResult(await _mediator.Send(new GetStudentFeeCounterQuery(studentId, academicYearId), ct));

    [HttpGet("{id:long}")]
    [HasPermission(PermissionNames.FeeInvoiceRead)]
    public async Task<IActionResult> GetById([FromRoute] long id, CancellationToken ct)
        => HandleResult(await _mediator.Send(new GetInvoiceByIdQuery(id), ct));

    // ----- Generation -----

    [HttpPost("generate")]
    [HasPermission(PermissionNames.FeeInvoiceCreate)]
    public async Task<IActionResult> Generate([FromBody] GenerateInvoiceRequest request, CancellationToken ct)
        => HandleResult(await _mediator.Send(new GenerateStudentInvoiceCommand(request), ct));

    [HttpPost("bulk-generate")]
    [HasPermission(PermissionNames.FeeInvoiceBulkGenerate)]
    public async Task<IActionResult> BulkGenerate([FromBody] BulkGenerateInvoicesRequest request, CancellationToken ct)
        => HandleResult(await _mediator.Send(new BulkGenerateInvoicesCommand(request), ct));

    [HttpPost("{id:long}/regenerate")]
    [HasPermission(PermissionNames.FeeInvoiceRegenerate)]
    public async Task<IActionResult> Regenerate([FromRoute] long id, CancellationToken ct)
        => HandleResult(await _mediator.Send(new RegenerateInvoiceCommand(id), ct));

    // ----- Adjustments -----

    [HttpPost("{id:long}/add-manual-line")]
    [HasPermission(PermissionNames.FeeInvoiceUpdate)]
    public async Task<IActionResult> AddManualLine([FromRoute] long id, [FromBody] AddManualLineRequest request, CancellationToken ct)
        => HandleResult(await _mediator.Send(new AddManualInvoiceLineCommand(id, request), ct));

    [HttpPost("{id:long}/adjust")]
    [HasPermission(PermissionNames.FeeInvoiceAdjust)]
    public async Task<IActionResult> Adjust([FromRoute] long id, [FromBody] AdjustInvoiceAmountRequest request, CancellationToken ct)
        => HandleResult(await _mediator.Send(new AdjustInvoiceAmountCommand(id, request), ct));

    [HttpPost("{id:long}/waive-late-fee")]
    [HasPermission(PermissionNames.FeeInvoiceWaiveLateFee)]
    public async Task<IActionResult> WaiveLateFee([FromRoute] long id, [FromBody] WaiveLateFeeRequest request, CancellationToken ct)
        => HandleResult(await _mediator.Send(new WaiveLateFeeCommand(id, request), ct));

    [HttpPost("recalculate-late-fees")]
    [HasPermission(PermissionNames.FeeInvoiceRecalculate)]
    public async Task<IActionResult> RecalculateLateFees([FromBody] RecalculateLateFeesRequest request, CancellationToken ct)
        => HandleResult(await _mediator.Send(new RecalculateLateFeesCommand(request), ct));

    // ----- Cancel (soft â€” no delete) -----

    [HttpDelete("{id:long}/cancel")]
    [HasPermission(PermissionNames.FeeInvoiceCancel)]
    public async Task<IActionResult> Cancel([FromRoute] long id, CancellationToken ct)
        => HandleResult(await _mediator.Send(new CancelInvoiceCommand(id), ct));

    // ----- Overdue status flip (manual) -----
    // Same handler used by OverdueInvoiceStatusJob (scheduled daily at 02:30 UTC).
    [HttpPost("mark-overdue")]
    [HasPermission(PermissionNames.FeeInvoiceRecalculate)]
    public async Task<IActionResult> MarkOverdue([FromQuery] long? academicYearId, CancellationToken ct)
        => HandleResult(await _mediator.Send(new MarkOverdueInvoicesCommand(academicYearId), ct));
}
