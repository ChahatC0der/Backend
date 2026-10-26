using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Fees.Builders;
using SchoolERP.Application.Features.Fees.DTOs.Invoice;
using SchoolERP.Domain.Fees.Enums;
using SchoolERP.Domain.Fees.ValueObjects;
using SchoolERP.Domain.Shared.Results;
using InvoiceEntity = SchoolERP.Domain.Fees.Entities.Invoice;
using InvoiceLineEntity = SchoolERP.Domain.Fees.Entities.InvoiceLine;
using LateFeeLogEntity = SchoolERP.Domain.Fees.Entities.LateFeeLog;

namespace SchoolERP.Application.Features.Fees.Commands.Invoice.RecalculateLateFees;

public sealed class RecalculateLateFeesCommandHandler : IRequestHandler<RecalculateLateFeesCommand, Result<RecalculateLateFeesResult>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;

    public RecalculateLateFeesCommandHandler(IApplicationDbContext db, ICurrentTenantService tenant, ICurrentBranchService branch)
    { _db = db; _tenant = tenant; _branch = branch; }

    public async Task<Result<RecalculateLateFeesResult>> Handle(RecalculateLateFeesCommand request, CancellationToken ct)
    {
        var scope = FeeScopeGuard.RequireBranch(_tenant, _branch);
        if (scope.IsFailure) return scope.Error!;
        var (tenantId, branchId) = scope.Value;

        var now = DateTime.UtcNow;
        var todayStart = now.Date;
        var tomorrowStart = todayStart.AddDays(1);

        IQueryable<InvoiceEntity> q = _db.Set<InvoiceEntity>().Include(i => i.Lines)
            .Where(i => i.TenantId == tenantId && i.BranchId == branchId && i.DeletedAt == null
                     && i.Status != InvoiceStatus.Cancelled && i.Status != InvoiceStatus.Paid);

        if (request.Request.InvoiceId.HasValue)
            q = q.Where(i => i.Id == request.Request.InvoiceId.Value);
        if (request.Request.AcademicYearId.HasValue)
            q = q.Where(i => i.AcademicYearId == request.Request.AcademicYearId.Value);

        var invoices = await q.ToListAsync(ct);

        if (request.Request.DryRun)
        {
            var toUpdate = invoices.Count(i => IsOverdue(i, now));
            return Result.Success(new RecalculateLateFeesResult(invoices.Count, toUpdate, invoices.Count - toUpdate, 0m, true));
        }

        var updated = 0;
        var skipped = 0;
        decimal totalAdded = 0m;

        var invoiceIds = invoices.Select(i => i.Id).ToList();
        var todayLogs = await _db.Set<LateFeeLogEntity>().AsNoTracking()
            .Where(l => invoiceIds.Contains(l.InvoiceId)
                     && l.CalculatedDate >= todayStart && l.CalculatedDate < tomorrowStart)
            .Select(l => l.InvoiceId)
            .ToListAsync(ct);
        var loggedTodaySet = todayLogs.ToHashSet();

        foreach (var invoice in invoices)
        {
            if (!IsOverdue(invoice, now)) { skipped++; continue; }
            if (loggedTodaySet.Contains(invoice.Id)) { skipped++; continue; }

            var beforeLateFee = invoice.LateFeeAmount;

            foreach (var sourceLine in invoice.Lines.Where(l => !l.IsLateFeeLine && l.LateFeeApplicable))
            {
                var rule = new LateFeeCalculator.RuleSnapshot(
                    sourceLine.LateFeeTypeSnapshot,
                    sourceLine.LateFeeValueSnapshot,
                    sourceLine.LateFeeGraceDaysSnapshot,
                    sourceLine.LateFeeCapPercentSnapshot,
                    sourceLine.LateFeeSlabConfigSnapshot,
                    sourceLine.LateFeeCapBase);

                var calculated = LateFeeCalculator.Calculate(rule, invoice.DueDate, now);

                var existingLine = invoice.Lines.FirstOrDefault(l =>
                    l.IsLateFeeLine && l.SourceInvoiceLineId == sourceLine.Id);

                if (existingLine is not null)
                {
                    existingLine.Amount = calculated;
                    existingLine.NetAmount = calculated;
                }
                else if (calculated > 0)
                {
                    invoice.Lines.Add(new InvoiceLineEntity
                    {
                        InvoiceId = invoice.Id,
                        FeeHeadId = sourceLine.FeeHeadId,
                        FeeHeadNameSnapshot = $"{sourceLine.FeeHeadNameSnapshot} (Late Fee)",
                        Amount = calculated,
                        ConcessionApplied = 0m,
                        NetAmount = calculated,
                        IsLateFeeLine = true,
                        SourceInvoiceLineId = sourceLine.Id,
                        LateFeeApplicable = false,
                        LateFeeCapBase = 0m,
                        CreatedAt = DateTime.UtcNow,
                    });
                }
            }

            invoice.LateFeeAmount = Money.Round(invoice.Lines.Where(l => l.IsLateFeeLine).Sum(l => l.Amount));
            invoice.NetPayable = Money.Round(invoice.TotalAmount - invoice.ConcessionAmount + invoice.LateFeeAmount);
            invoice.UpdatedAt = DateTime.UtcNow;

            var delta = invoice.LateFeeAmount - beforeLateFee;
            if (delta != 0m) totalAdded += delta;

            _db.Set<LateFeeLogEntity>().Add(new LateFeeLogEntity
            {
                InvoiceId = invoice.Id,
                CalculatedDate = now,
                LateFeeAmount = invoice.LateFeeAmount,
                WaivedAmount = 0m,
                CreatedAt = DateTime.UtcNow,
            });

            updated++;
        }

        if (updated > 0)
        {
            await _db.SaveChangesAsync(ct);
            // TODO (audit): await _auditWriter.LogAsync("Invoices", 0, FinancialAuditAction.Update, null, new { Updated = updated, TotalLateFeeAdded = totalAdded }, ct);
        }

        return Result.Success(new RecalculateLateFeesResult(invoices.Count, updated, skipped, totalAdded, false));
    }

    private static bool IsOverdue(InvoiceEntity invoice, DateTime asOf)
    {
        var outstanding = invoice.NetPayable - (invoice.PaidAmount + invoice.CreditAppliedAmount);
        return invoice.DueDate < asOf && outstanding > 0;
    }
}
