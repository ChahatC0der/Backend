using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Fees.DTOs.Invoice;
using SchoolERP.Domain.Fees.Enums;
using SchoolERP.Domain.Fees.ValueObjects;
using SchoolERP.Domain.Shared.Results;
using InvoiceEntity = SchoolERP.Domain.Fees.Entities.Invoice;
using LateFeeLogEntity = SchoolERP.Domain.Fees.Entities.LateFeeLog;

namespace SchoolERP.Application.Features.Fees.Commands.Invoice.WaiveLateFee;

public sealed class WaiveLateFeeCommandHandler : IRequestHandler<WaiveLateFeeCommand, Result<InvoiceResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;
    private readonly ICurrentUserService _user;

    public WaiveLateFeeCommandHandler(IApplicationDbContext db, ICurrentTenantService tenant,
                                      ICurrentBranchService branch, ICurrentUserService user)
    { _db = db; _tenant = tenant; _branch = branch; _user = user; }

    public async Task<Result<InvoiceResponse>> Handle(WaiveLateFeeCommand request, CancellationToken ct)
    {
        var scope = FeeScopeGuard.RequireBranch(_tenant, _branch);
        if (scope.IsFailure) return scope.Error!;
        var (tenantId, branchId) = scope.Value;

        var invoice = await _db.Set<InvoiceEntity>().Include(i => i.Lines).FirstOrDefaultAsync(i =>
            i.Id == request.InvoiceId && i.TenantId == tenantId && i.BranchId == branchId && i.DeletedAt == null, ct);
        if (invoice is null) return Error.NotFound("Invoice", request.InvoiceId.ToString());

        if (invoice.Status is InvoiceStatus.Paid or InvoiceStatus.Cancelled)
            return Error.Conflict($"Cannot waive late fee on a {invoice.Status} invoice.");
        if (invoice.LateFeeAmount <= 0)
            return Error.Validation("Invoice has no late fee to waive.");

        var waiveAmount = request.Request.WaiveAmount ?? invoice.LateFeeAmount;
        if (waiveAmount > invoice.LateFeeAmount)
            return Error.Validation($"Waive amount exceeds current late fee of {invoice.LateFeeAmount}.");

        var remaining = waiveAmount;
        foreach (var line in invoice.Lines.Where(l => l.IsLateFeeLine && l.Amount > 0).OrderBy(l => l.Id))
        {
            if (remaining <= 0) break;
            var take = Math.Min(remaining, line.Amount);
            line.Amount = Money.Round(line.Amount - take);
            line.NetAmount = line.Amount;
            remaining = Money.Round(remaining - take);
        }

        var todayStart = DateTime.UtcNow.Date;
        var tomorrowStart = todayStart.AddDays(1);
        var log = await _db.Set<LateFeeLogEntity>().FirstOrDefaultAsync(l =>
            l.InvoiceId == invoice.Id &&
            l.CalculatedDate >= todayStart && l.CalculatedDate < tomorrowStart, ct);

        if (log is null)
        {
            log = new LateFeeLogEntity
            {
                InvoiceId = invoice.Id,
                CalculatedDate = DateTime.UtcNow,
                LateFeeAmount = invoice.LateFeeAmount,
                WaivedAmount = waiveAmount,
                WaivedBy = _user.GetUserId(),
                WaiverReason = request.Request.WaiverReason.Trim(),
                CreatedAt = DateTime.UtcNow,
            };
            _db.Set<LateFeeLogEntity>().Add(log);
        }
        else
        {
            log.WaivedAmount = Money.Round((log.WaivedAmount ?? 0m) + waiveAmount);
            log.WaivedBy = _user.GetUserId();
            log.WaiverReason = request.Request.WaiverReason.Trim();
            log.UpdatedAt = DateTime.UtcNow;
        }

        invoice.LateFeeAmount = Money.Round(invoice.Lines.Where(l => l.IsLateFeeLine).Sum(l => l.Amount));
        invoice.NetPayable = Money.Round(invoice.TotalAmount - invoice.ConcessionAmount + invoice.LateFeeAmount);
        invoice.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        // TODO (audit): await _auditWriter.LogAsync("Invoices", invoice.Id, FinancialAuditAction.Waive, null, new { Waived = waiveAmount, Reason = request.Request.WaiverReason }, ct);

        return Result.Success(InvoiceResponseFactory.ToResponse(invoice));
    }
}
