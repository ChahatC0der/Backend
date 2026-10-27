using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Fees.Enums;
using SchoolERP.Domain.Shared.Results;
using InvoiceEntity = SchoolERP.Domain.Fees.Entities.Invoice;

namespace SchoolERP.Application.Features.Fees.Commands.Invoice.MarkOverdueInvoices;

public sealed class MarkOverdueInvoicesCommandHandler : IRequestHandler<MarkOverdueInvoicesCommand, Result<int>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;

    public MarkOverdueInvoicesCommandHandler(
        IApplicationDbContext db, ICurrentTenantService tenant, ICurrentBranchService branch)
    { _db = db; _tenant = tenant; _branch = branch; }

    public async Task<Result<int>> Handle(MarkOverdueInvoicesCommand request, CancellationToken ct)
    {
        var scope = FeeScopeGuard.RequireBranch(_tenant, _branch);
        if (scope.IsFailure) return scope.Error!;
        var (tenantId, branchId) = scope.Value;

        var now = DateTime.UtcNow;

        // --- Read everything into memory first (validate-all-before-mutate) ---
        var query = _db.Set<InvoiceEntity>().Where(i =>
            i.TenantId == tenantId
            && i.BranchId == branchId
            && i.DeletedAt == null
            && (i.Status == InvoiceStatus.Sent || i.Status == InvoiceStatus.PartiallyPaid)
            && i.DueDate < now
            && (i.NetPayable - i.PaidAmount - i.CreditAppliedAmount) > 0);

        if (request.AcademicYearId.HasValue)
            query = query.Where(i => i.AcademicYearId == request.AcademicYearId.Value);

        var candidates = await query.ToListAsync(ct);

        if (candidates.Count == 0)
            return Result.Success(0);

        // --- Mutations ---
        foreach (var inv in candidates)
        {
            inv.Status = InvoiceStatus.Overdue;
            inv.UpdatedAt = now;
        }

        await _db.SaveChangesAsync(ct);

        // TODO (audit): await _auditWriter.LogAsync("Invoices", 0, FinancialAuditAction.Update, null, new { FlippedToOverdue = candidates.Count }, ct);

        return Result.Success(candidates.Count);
    }
}
