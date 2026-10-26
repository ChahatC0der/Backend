using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Fees.Enums;
using SchoolERP.Domain.Shared.Results;
using InvoiceEntity = SchoolERP.Domain.Fees.Entities.Invoice;

namespace SchoolERP.Application.Features.Fees.Commands.Invoice.CancelInvoice;

public sealed class CancelInvoiceCommandHandler : IRequestHandler<CancelInvoiceCommand, Result<bool>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;

    public CancelInvoiceCommandHandler(IApplicationDbContext db, ICurrentTenantService tenant, ICurrentBranchService branch)
    { _db = db; _tenant = tenant; _branch = branch; }

    public async Task<Result<bool>> Handle(CancelInvoiceCommand request, CancellationToken ct)
    {
        var scope = FeeScopeGuard.RequireBranch(_tenant, _branch);
        if (scope.IsFailure) return scope.Error!;
        var (tenantId, branchId) = scope.Value;

        var invoice = await _db.Set<InvoiceEntity>().FirstOrDefaultAsync(i =>
            i.Id == request.InvoiceId && i.TenantId == tenantId && i.BranchId == branchId && i.DeletedAt == null, ct);
        if (invoice is null) return Error.NotFound("Invoice", request.InvoiceId.ToString());

        if (invoice.Status == InvoiceStatus.Paid)
            return Error.Conflict("Paid invoices must be refunded, not cancelled.");
        if (invoice.Status == InvoiceStatus.Cancelled)
            return Error.Conflict("Invoice is already cancelled.");

        invoice.Status = InvoiceStatus.Cancelled;
        invoice.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        // TODO (audit): await _auditWriter.LogAsync("Invoices", invoice.Id, FinancialAuditAction.Cancel, null, null, ct);
        return Result.Success(true);
    }
}
