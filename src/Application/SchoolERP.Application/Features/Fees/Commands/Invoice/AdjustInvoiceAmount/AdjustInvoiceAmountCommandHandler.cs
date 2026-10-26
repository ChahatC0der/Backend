using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Fees.DTOs.Invoice;
using SchoolERP.Domain.Fees.Enums;
using SchoolERP.Domain.Fees.ValueObjects;
using SchoolERP.Domain.Shared.Results;
using InvoiceEntity = SchoolERP.Domain.Fees.Entities.Invoice;

namespace SchoolERP.Application.Features.Fees.Commands.Invoice.AdjustInvoiceAmount;

public sealed class AdjustInvoiceAmountCommandHandler : IRequestHandler<AdjustInvoiceAmountCommand, Result<InvoiceResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;

    public AdjustInvoiceAmountCommandHandler(IApplicationDbContext db, ICurrentTenantService tenant, ICurrentBranchService branch)
    { _db = db; _tenant = tenant; _branch = branch; }

    public async Task<Result<InvoiceResponse>> Handle(AdjustInvoiceAmountCommand request, CancellationToken ct)
    {
        var scope = FeeScopeGuard.RequireBranch(_tenant, _branch);
        if (scope.IsFailure) return scope.Error!;
        var (tenantId, branchId) = scope.Value;

        var invoice = await _db.Set<InvoiceEntity>().Include(i => i.Lines).FirstOrDefaultAsync(i =>
            i.Id == request.InvoiceId && i.TenantId == tenantId && i.BranchId == branchId && i.DeletedAt == null, ct);
        if (invoice is null) return Error.NotFound("Invoice", request.InvoiceId.ToString());

        if (invoice.Status is InvoiceStatus.Paid or InvoiceStatus.Cancelled)
            return Error.Conflict($"Cannot adjust a {invoice.Status} invoice.");

        var line = invoice.Lines.FirstOrDefault(l => l.Id == request.Request.InvoiceLineId);
        if (line is null) return Error.NotFound("InvoiceLine", request.Request.InvoiceLineId.ToString());

        if (line.IsLateFeeLine)
            return Error.Conflict("Late-fee lines cannot be adjusted; use WaiveLateFee instead.");

        var oldAmount = line.Amount;
        line.Amount = request.Request.NewAmount;
        line.NetAmount = Money.Round(request.Request.NewAmount - line.ConcessionApplied);
        line.LateFeeCapBase = line.NetAmount;

        invoice.TotalAmount = Money.Round(invoice.TotalAmount + (request.Request.NewAmount - oldAmount));
        invoice.NetPayable = Money.Round(invoice.TotalAmount - invoice.ConcessionAmount + invoice.LateFeeAmount);
        invoice.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        // TODO (audit): await _auditWriter.LogAsync("Invoices", invoice.Id, FinancialAuditAction.Adjust, new { OldAmount = oldAmount }, new { NewAmount = request.Request.NewAmount, Reason = request.Request.Reason }, ct);

        return Result.Success(InvoiceResponseFactory.ToResponse(invoice));
    }
}
