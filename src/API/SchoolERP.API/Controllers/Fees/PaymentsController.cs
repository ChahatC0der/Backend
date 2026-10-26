using MediatR;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.API.Authorization;
using SchoolERP.Application.Features.Fees.Commands.Payment.ApplyCreditToInvoice;
using SchoolERP.Application.Features.Fees.Commands.Payment.BulkRecordPaymentsCsv;
using SchoolERP.Application.Features.Fees.Commands.Payment.CollectPayment;
using SchoolERP.Application.Features.Fees.Commands.Payment.MarkChequeBounced;
using SchoolERP.Application.Features.Fees.Commands.Payment.RecordAdvancePayment;
using SchoolERP.Application.Features.Fees.Commands.Payment.TransferStudentCredit;
using SchoolERP.Application.Features.Fees.DTOs.Payment;
using SchoolERP.Application.Features.Fees.Queries.Payment.GetPaymentReceipt;
using SchoolERP.Application.Features.Fees.Queries.Payment.GetPayments;
using SchoolERP.Application.Features.Fees.Queries.Payment.GetStudentCreditHistory;
using SchoolERP.Domain.Shared.Permissions;

namespace SchoolERP.API.Controllers.Fees;

[ApiController]
[Route("api/fees/payments")]
public sealed class PaymentsController : BaseApiController
{
    private readonly IMediator _mediator;
    public PaymentsController(IMediator mediator) => _mediator = mediator;

    // ----- Reads -----

    [HttpGet]
    [HasPermission(PermissionNames.FeePaymentRead)]
    public async Task<IActionResult> Get([FromQuery] GetPaymentsQuery query, CancellationToken ct)
        => HandleResult(await _mediator.Send(query, ct));

    [HttpGet("{id:long}/receipt")]
    [HasPermission(PermissionNames.FeePaymentReceipt)]
    public async Task<IActionResult> GetReceipt([FromRoute] long id, CancellationToken ct)
        => HandleResult(await _mediator.Send(new GetPaymentReceiptQuery(id), ct));

    [HttpGet("wallet/{studentId:long}")]
    [HasPermission(PermissionNames.FeePaymentRead)]
    public async Task<IActionResult> GetWallet([FromRoute] long studentId, CancellationToken ct)
        => HandleResult(await _mediator.Send(new GetStudentCreditHistoryQuery(studentId), ct));

    // ----- Writes -----

    [HttpPost("collect")]
    [HasPermission(PermissionNames.FeePaymentCollect)]
    public async Task<IActionResult> Collect([FromBody] CollectPaymentRequest request, CancellationToken ct)
        => HandleResult(await _mediator.Send(new CollectPaymentCommand(request), ct));

    [HttpPost("advance")]
    [HasPermission(PermissionNames.FeePaymentAdvance)]
    public async Task<IActionResult> Advance([FromBody] AdvancePaymentRequest request, CancellationToken ct)
        => HandleResult(await _mediator.Send(new RecordAdvancePaymentCommand(request), ct));

    [HttpPost("apply-credit")]
    [HasPermission(PermissionNames.FeePaymentApplyCredit)]
    public async Task<IActionResult> ApplyCredit([FromBody] ApplyCreditRequest request, CancellationToken ct)
        => HandleResult(await _mediator.Send(new ApplyCreditToInvoiceCommand(request), ct));

    [HttpPost("transfer-credit")]
    [HasPermission(PermissionNames.FeePaymentTransferCredit)]
    public async Task<IActionResult> TransferCredit([FromBody] TransferCreditRequest request, CancellationToken ct)
        => HandleResult(await _mediator.Send(new TransferStudentCreditCommand(request), ct));

    [HttpPost("bulk-csv")]
    [HasPermission(PermissionNames.FeePaymentBulkCsv)]
    public async Task<IActionResult> BulkCsv([FromQuery] long academicYearId, [FromBody] List<BulkCsvRowRequest> rows, CancellationToken ct)
        => HandleResult(await _mediator.Send(new BulkRecordPaymentsCsvCommand(academicYearId, rows), ct));

    [HttpPost("{id:long}/cheque-bounced")]
    [HasPermission(PermissionNames.FeePaymentBounce)]
    public async Task<IActionResult> ChequeBounced([FromRoute] long id, [FromQuery] string reason, CancellationToken ct)
        => HandleResult(await _mediator.Send(new MarkChequeBouncedCommand(id, reason), ct));
}
