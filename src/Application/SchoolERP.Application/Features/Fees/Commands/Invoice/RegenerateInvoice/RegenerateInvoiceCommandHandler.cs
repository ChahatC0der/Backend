using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Fees.Builders;
using SchoolERP.Application.Features.Fees.DTOs.Invoice;
using SchoolERP.Domain.Fees.Entities;
using SchoolERP.Domain.Fees.Enums;
using SchoolERP.Domain.Shared.Results;
using ConcessionUsageEntity = SchoolERP.Domain.Fees.Entities.ConcessionUsage;
using InvoiceEntity = SchoolERP.Domain.Fees.Entities.Invoice;
using InvoiceLineEntity = SchoolERP.Domain.Fees.Entities.InvoiceLine;

namespace SchoolERP.Application.Features.Fees.Commands.Invoice.RegenerateInvoice;

public sealed class RegenerateInvoiceCommandHandler : IRequestHandler<RegenerateInvoiceCommand, Result<InvoiceResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;
    private readonly ICurrentAcademicYearProvider _ay;
    private readonly IFeeSequenceGenerator _sequence;
    private readonly ITransportFeeQuoteProvider _transportProvider;

    public RegenerateInvoiceCommandHandler(
        IApplicationDbContext db, ICurrentTenantService tenant, ICurrentBranchService branch,
        ICurrentAcademicYearProvider ay, IFeeSequenceGenerator sequence,
        ITransportFeeQuoteProvider transportProvider)
    {
        _db = db; _tenant = tenant; _branch = branch;
        _ay = ay; _sequence = sequence; _transportProvider = transportProvider;
    }

    public async Task<Result<InvoiceResponse>> Handle(RegenerateInvoiceCommand request, CancellationToken ct)
    {
        var scope = FeeScopeGuard.RequireBranch(_tenant, _branch);
        if (scope.IsFailure) return scope.Error!;
        var (tenantId, branchId) = scope.Value;

        // ---------------------------------------------------------
        // BLOCK: Load the source invoice. Tracked (NOT AsNoTracking)
        // because we mutate its status inside the transaction.
        // ---------------------------------------------------------
        var old = await _db.Set<InvoiceEntity>()
            .Include(i => i.Lines)
            .FirstOrDefaultAsync(i =>
                i.Id == request.InvoiceId &&
                i.TenantId == tenantId &&
                i.BranchId == branchId &&
                i.DeletedAt == null, ct);
        if (old is null) return Error.NotFound("Invoice", request.InvoiceId.ToString());

        // ---------------------------------------------------------
        // BLOCK: Regeneration guards.
        //
        // 1) Terminal statuses are untouchable.
        // 2) ANY money movement on the invoice blocks regeneration:
        //    cancelling a PartlyPaid invoice would orphan its payments
        //    and rebirth a zero-paid Draft. This is stricter than the
        //    old 'Status == Paid' check on purpose.
        // ---------------------------------------------------------
        if (old.Status == InvoiceStatus.Paid)
            return Error.Conflict("Cannot regenerate a paid invoice.");
        if (old.Status == InvoiceStatus.Cancelled)
            return Error.Conflict("Cannot regenerate a cancelled invoice.");
        if (old.PaidAmount > 0m || old.CreditAppliedAmount > 0m)
            return Error.Conflict(
                "Cannot regenerate an invoice with payments or credit already applied. " +
                "Cancel it and adjust manually instead.");

        // ---------------------------------------------------------
        // BLOCK: Regeneration only supports PERIODIC invoices.
        // Manual / ADMISSION invoices (null PeriodSequenceNumber) have
        // no billing period to rebuild through the period pipeline.
        // ---------------------------------------------------------
        if (!old.PeriodSequenceNumber.HasValue)
            return Error.Conflict(
                "Only periodic invoices can be regenerated. " +
                "Manual invoices must be cancelled and re-issued.");

        var ayInfo = await _ay.GetByIdAsync(old.AcademicYearId, ct);
        if (ayInfo is null) return Error.NotFound("AcademicYear", old.AcademicYearId.ToString());

        // ---------------------------------------------------------
        // BLOCK: Resolve the billing period exactly like generation:
        // tenant frequency -> BillingPeriodCalculator -> match on the
        // frozen PeriodSequenceNumber. Deterministic: same AY +
        // same frequency always yields the same period, so the
        // rebuilt draft carries identical period dates/EndDate due date.
        // ---------------------------------------------------------
        var frequency = InvoiceGenerationFrequencyResolver.Resolve(tenantId);
        var billingPeriods = BillingPeriodCalculator.Calculate(ayInfo.StartDate, ayInfo.EndDate, frequency);
        var billingPeriod = billingPeriods
            .FirstOrDefault(p => p.SequenceNumber == old.PeriodSequenceNumber.Value);

        if (billingPeriod is null)
            return Error.Conflict(
                $"Period #{old.PeriodSequenceNumber} of this invoice no longer exists " +
                $"under the tenant's current frequency '{frequency}'. " +
                "Frequency changes require manual re-issue instead of regeneration.");

        var periodLabel = PeriodLabelBuilder.ForBillingPeriod(billingPeriod);

        // ---------------------------------------------------------
        // Generation timestamp only. DueDate is builder-owned
        // (= billingPeriod.EndDate) — NOT copied from the old invoice.
        // ---------------------------------------------------------
        var generationDate = DateTime.UtcNow;

        // ---------------------------------------------------------
        // BLOCK: Unit of work — cancel old + issue new + lines +
        // concession usages in ONE transaction. The context owns the
        // transaction (begin/commit/rollback via context methods),
        // same as the single and bulk handlers.
        // ---------------------------------------------------------
        await _db.BeginTransactionAsync(ct);
        try
        {
            // 1) Cancel the old invoice — same invoice number stays on it
            //    for the audit trail; the new one gets a fresh number.
            old.Status = InvoiceStatus.Cancelled;
            old.UpdatedAt = DateTime.UtcNow;

            // 2) AB nayi invoice build karo — ab loader ko purani invoice 'Cancelled' milegi
            //    isliye OneTime (Admission ₹2,500) wapas nayi invoice mein aa jayegi!
            var draftResult = await InvoiceBuilder.BuildForPeriodAsync(
                db: _db,
                branchId: branchId,
                studentId: old.StudentId,
                academicYearId: old.AcademicYearId,
                academicYearStartDate: ayInfo.StartDate,
                academicYearEndDate: ayInfo.EndDate,
                billingPeriod: billingPeriod,
                periodLabel: periodLabel,
                generationDate: generationDate,
                transportProvider: _transportProvider,
                ct: ct);

            if (draftResult.IsFailure)
            {
                await _db.RollbackTransactionAsync(CancellationToken.None);
                return draftResult.Error!;
            }

            var draft = draftResult.Value;

            var invoiceNumber = await _sequence.NextInvoiceNumberAsync(branchId, ayInfo.Code, ct);

            var invoice = new InvoiceEntity
            {
                TenantId = tenantId,
                BranchId = branchId,
                StudentId = draft.StudentId,
                AcademicYearId = draft.AcademicYearId,
                ClassId = draft.ClassId,
                SectionId = draft.SectionId,
                FeeStructureId = draft.FeeStructureId,
                InvoiceNumber = invoiceNumber,
                PeriodLabel = draft.PeriodLabel,
                // Billing period tracking — the SAME period the old
                // invoice occupied, taking its place after cancellation.
                PeriodSequenceNumber = billingPeriod.SequenceNumber,
                PeriodStartDate = billingPeriod.StartDate,
                PeriodEndDate = billingPeriod.EndDate,
                GenerationDate = draft.GenerationDate,
                DueDate = draft.DueDate, // = billingPeriod.EndDate, builder-owned
                TotalAmount = draft.TotalAmount,
                ConcessionAmount = draft.ConcessionAmount,
                LateFeeAmount = 0m,
                NetPayable = draft.NetPayable,
                PaidAmount = 0m,
                CreditAppliedAmount = 0m,
                Status = InvoiceStatus.Draft,
                // Header late-fee snapshots — re-frozen at rebirth from the
                // CURRENT FeeStructure header (regeneration is the one legit
                // reason snapshots change: structure edits since the first issue).
                LateFeeModeSnapshot = draft.LateFeeModeSnapshot,
                InvoiceLateFeeTypeSnapshot = draft.InvoiceLateFeeTypeSnapshot,
                InvoiceLateFeeValueSnapshot = draft.InvoiceLateFeeValueSnapshot,
                InvoiceLateFeeGraceDaysSnapshot = draft.InvoiceLateFeeGraceDaysSnapshot,
                InvoiceLateFeeCapPercentSnapshot = draft.InvoiceLateFeeCapPercentSnapshot,
                InvoiceLateFeeSlabConfigSnapshot = draft.InvoiceLateFeeSlabConfigSnapshot,
                InvoiceLateFeeCapBase = draft.InvoiceLateFeeCapBase,
                CreatedAt = DateTime.UtcNow,
            };

            foreach (var d in draft.Lines)
            {
                invoice.Lines.Add(new InvoiceLineEntity
                {
                    FeeHeadId = d.FeeHeadId,
                    SourceFeeStructureLineId = d.SourceFeeStructureLineId,
                    FeeHeadNameSnapshot = d.FeeHeadNameSnapshot,
                    Amount = d.Amount,
                    ConcessionApplied = d.ConcessionApplied,
                    NetAmount = d.NetAmount,
                    IsLateFeeLine = false,
                    LateFeeApplicable = d.LateFeeApplicable,
                    LateFeeTypeSnapshot = d.LateFeeTypeSnapshot,
                    LateFeeValueSnapshot = d.LateFeeValueSnapshot,
                    LateFeeGraceDaysSnapshot = d.LateFeeGraceDaysSnapshot,
                    LateFeeCapPercentSnapshot = d.LateFeeCapPercentSnapshot,
                    LateFeeSlabConfigSnapshot = d.LateFeeSlabConfigSnapshot,
                    LateFeeCapBase = d.LateFeeCapBase,
                    CreatedAt = DateTime.UtcNow,
                });
            }

            _db.Set<InvoiceEntity>().Add(invoice);
            await _db.SaveChangesAsync(ct);

            // 2) Concession usage tracking for the NEW invoice — one row
            //    per (allocation, invoice), same as generation handlers.
            if (draft.ConcessionUsages.Count > 0)
            {
                var usages = draft.ConcessionUsages.Select(u => new ConcessionUsageEntity
                {
                    ConcessionAllocationId = u.ConcessionAllocationId,
                    InvoiceId = invoice.Id,
                    AppliedAmount = u.AppliedAmount,
                    CreatedAt = DateTime.UtcNow,
                }).ToList();
                _db.Set<ConcessionUsageEntity>().AddRange(usages);
                await _db.SaveChangesAsync(ct);
            }

            await _db.CommitTransactionAsync(ct);
        return Result.Success(InvoiceResponseFactory.ToResponse(invoice));
        }
        catch
        {
            await _db.RollbackTransactionAsync(CancellationToken.None);
            throw;
        }

    }
}