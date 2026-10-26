using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Fees.DTOs.Invoice;
using SchoolERP.Domain.Fees.Enums;
using SchoolERP.Domain.Fees.ValueObjects;
using SchoolERP.Domain.Shared.Results;
using InvoiceEntity = SchoolERP.Domain.Fees.Entities.Invoice;
using InvoiceLineEntity = SchoolERP.Domain.Fees.Entities.InvoiceLine;
using FeeHeadEntity = SchoolERP.Domain.Fees.Entities.FeeHead;

namespace SchoolERP.Application.Features.Fees.Commands.Invoice.AddManualInvoiceLine;

public sealed class AddManualInvoiceLineCommandHandler : IRequestHandler<AddManualInvoiceLineCommand, Result<InvoiceResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;

    public AddManualInvoiceLineCommandHandler(IApplicationDbContext db, ICurrentTenantService tenant, ICurrentBranchService branch)
    { _db = db; _tenant = tenant; _branch = branch; }

    public async Task<Result<InvoiceResponse>> Handle(AddManualInvoiceLineCommand request, CancellationToken ct)
    {
        var scope = FeeScopeGuard.RequireBranch(_tenant, _branch);
        if (scope.IsFailure) return scope.Error!;
        var (tenantId, branchId) = scope.Value;

        var invoice = await _db.Set<InvoiceEntity>().Include(i => i.Lines).FirstOrDefaultAsync(i =>
            i.Id == request.InvoiceId && i.TenantId == tenantId && i.BranchId == branchId && i.DeletedAt == null, ct);
        if (invoice is null) return Error.NotFound("Invoice", request.InvoiceId.ToString());

        if (invoice.Status is InvoiceStatus.Paid or InvoiceStatus.Cancelled)
            return Error.Conflict($"Cannot add lines to a {invoice.Status} invoice.");

        var feeHead = await _db.Set<FeeHeadEntity>().FirstOrDefaultAsync(h =>
            h.Id == request.Request.FeeHeadId && h.TenantId == tenantId && h.BranchId == branchId && h.DeletedAt == null, ct);
        if (feeHead is null) return Error.NotFound("FeeHead", request.Request.FeeHeadId.ToString());

        invoice.Lines.Add(new InvoiceLineEntity
        {
            FeeHeadId = feeHead.Id,
            FeeHeadNameSnapshot = feeHead.Name,
            Amount = request.Request.Amount,
            ConcessionApplied = 0m,
            NetAmount = request.Request.Amount,
            IsLateFeeLine = false,
            LateFeeApplicable = false,
            LateFeeCapBase = request.Request.Amount,
            CreatedAt = DateTime.UtcNow,
        });

        invoice.TotalAmount = Money.Round(invoice.TotalAmount + request.Request.Amount);
        invoice.NetPayable = Money.Round(invoice.TotalAmount - invoice.ConcessionAmount + invoice.LateFeeAmount);
        invoice.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        // TODO (audit): await _auditWriter.LogAsync("Invoices", invoice.Id, FinancialAuditAction.Update, null, new { AddedLine = feeHead.Name, Amount = request.Request.Amount }, ct);

        return Result.Success(InvoiceResponseFactory.ToResponse(invoice));
    }
}
