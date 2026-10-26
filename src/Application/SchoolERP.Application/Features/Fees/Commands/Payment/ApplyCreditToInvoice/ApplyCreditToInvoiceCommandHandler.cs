using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Fees.DTOs.Invoice;
using SchoolERP.Domain.Fees.Enums;
using SchoolERP.Domain.Fees.ValueObjects;
using SchoolERP.Domain.Shared.Results;
using InvoiceEntity = SchoolERP.Domain.Fees.Entities.Invoice;

namespace SchoolERP.Application.Features.Fees.Commands.Payment.ApplyCreditToInvoice;

public sealed class ApplyCreditToInvoiceCommandHandler : IRequestHandler<ApplyCreditToInvoiceCommand, Result<InvoiceResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;
    private readonly IWalletLedgerWriter _wallet;

    public ApplyCreditToInvoiceCommandHandler(
        IApplicationDbContext db, ICurrentTenantService tenant, ICurrentBranchService branch,
        IWalletLedgerWriter wallet)
    { _db = db; _tenant = tenant; _branch = branch; _wallet = wallet; }

    public async Task<Result<InvoiceResponse>> Handle(ApplyCreditToInvoiceCommand request, CancellationToken ct)
    {
        var scope = FeeScopeGuard.RequireBranch(_tenant, _branch);
        if (scope.IsFailure) return scope.Error!;
        var (tenantId, branchId) = scope.Value;

        var invoice = await _db.Set<InvoiceEntity>().Include(i => i.Lines).FirstOrDefaultAsync(i =>
            i.Id == request.Request.InvoiceId && i.TenantId == tenantId
            && i.BranchId == branchId && i.DeletedAt == null, ct);
        if (invoice is null) return Error.NotFound("Invoice", request.Request.InvoiceId.ToString());

        if (invoice.Status is InvoiceStatus.Paid or InvoiceStatus.Cancelled)
            return Error.Conflict($"Cannot apply credit to a {invoice.Status} invoice.");

        var outstanding = Money.Round(invoice.NetPayable - (invoice.PaidAmount + invoice.CreditAppliedAmount));
        if (outstanding <= 0) return Error.Conflict("Invoice has no outstanding balance.");
        if (request.Request.AmountToApply > outstanding)
            return Error.Validation($"Amount exceeds outstanding of {outstanding}.");

        var walletResult = await _wallet.ApplyAsync(new WalletOperation(
            StudentId: invoice.StudentId,
            TenantId: tenantId,
            AcademicYearId: invoice.AcademicYearId,
            Delta: -request.Request.AmountToApply,
            Type: CreditTransactionType.ManualAdjustment,
            ReferenceInvoiceId: invoice.Id,
            Reason: $"Applied to invoice {invoice.InvoiceNumber}"), ct);
        if (walletResult.IsFailure) return walletResult.Error!;

        invoice.CreditAppliedAmount = Money.Round(invoice.CreditAppliedAmount + request.Request.AmountToApply);
        invoice.Status = invoice.PaidAmount + invoice.CreditAppliedAmount >= invoice.NetPayable
            ? InvoiceStatus.Paid
            : InvoiceStatus.PartiallyPaid;
        invoice.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        // TODO (audit): await _auditWriter.LogAsync("Invoices", invoice.Id, FinancialAuditAction.Adjust, null, new { CreditApplied = request.Request.AmountToApply }, ct);

        return Result.Success(InvoiceResponseFactory.ToResponse(invoice));
    }
}
