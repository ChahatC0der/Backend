using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Fees.Builders;
using SchoolERP.Application.Features.Fees.DTOs.Invoice;
using SchoolERP.Domain.Fees.Enums;
using SchoolERP.Domain.Shared.Results;
using InvoiceEntity = SchoolERP.Domain.Fees.Entities.Invoice;
using InvoiceLineEntity = SchoolERP.Domain.Fees.Entities.InvoiceLine;
using ConcessionUsageEntity = SchoolERP.Domain.Fees.Entities.ConcessionUsage;
using StudentEnrollmentEntity = SchoolERP.Domain.Student.Entities.StudentEnrollment;

namespace SchoolERP.Application.Features.Fees.Commands.Invoice.BulkGenerateInvoices;

public sealed class BulkGenerateInvoicesCommandHandler : IRequestHandler<BulkGenerateInvoicesCommand, Result<BulkGenerateResult>>
{
    private const int MaxStudentsPerRequest = 500;
    private const int MaxPeriodsPerRequest = 12;

    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;
    private readonly ICurrentAcademicYearProvider _ay;
    private readonly IFeeSequenceGenerator _sequence;
    private readonly ITransportFeeQuoteProvider _transportProvider;

    public BulkGenerateInvoicesCommandHandler(
        IApplicationDbContext db, ICurrentTenantService tenant, ICurrentBranchService branch,
        ICurrentAcademicYearProvider ay, IFeeSequenceGenerator sequence,
        ITransportFeeQuoteProvider transportProvider)
    {
        _db = db; _tenant = tenant; _branch = branch;
        _ay = ay; _sequence = sequence; _transportProvider = transportProvider;
    }

    public async Task<Result<BulkGenerateResult>> Handle(BulkGenerateInvoicesCommand request, CancellationToken ct)
    {
        var scope = FeeScopeGuard.RequireBranch(_tenant, _branch);
        if (scope.IsFailure) return scope.Error!;
        var (tenantId, branchId) = scope.Value;

        var ayInfo = await _ay.GetByIdAsync(request.Request.AcademicYearId, ct);
        if (ayInfo is null) return Error.NotFound("AcademicYear", request.Request.AcademicYearId.ToString());

        // ---------------------------------------------------------
        // BLOCK: Resolve tenant frequency and all billing periods
        // for the Academic Year — identical to the single handler.
        // ---------------------------------------------------------
        var frequency = InvoiceGenerationFrequencyResolver.Resolve(tenantId);
        var billingPeriods = BillingPeriodCalculator.Calculate(ayInfo.StartDate, ayInfo.EndDate, frequency);
        var periodBySequence = billingPeriods.ToDictionary(p => p.SequenceNumber);

        // ---------------------------------------------------------
        // BLOCK: Requested periods — MULTIPLE sequences in one run.
        // Distinct + ascending. Every requested sequence must exist
        // under the tenant frequency for this Academic Year.
        // ---------------------------------------------------------
        var requestedSequences = request.Request.PeriodSequenceNumbers
            .Distinct()
            .OrderBy(s => s)
            .ToList();

        if (requestedSequences.Count == 0)
            return Error.Validation("At least one PeriodSequenceNumber is required.");

        if (requestedSequences.Count > MaxPeriodsPerRequest)
            return Error.Validation($"Bulk generation is capped at {MaxPeriodsPerRequest} periods per request in Phase 1.");

        var unknown = requestedSequences.Where(s => !periodBySequence.ContainsKey(s)).ToList();
        if (unknown.Count > 0)
            return Error.Validation(
                $"Invalid period sequence number(s): {string.Join(", ", unknown)}. " +
                $"Frequency {frequency} allows {billingPeriods.Count} periods " +
                $"(1..{billingPeriods.Count}) in this Academic Year.");

        // Generation timestamp only. DueDate is builder-owned (= period EndDate).
        var generationDate = DateTime.UtcNow;

        // ---------------------------------------------------------
        // BLOCK: Target students (Phase 1 cap of 500 per request).
        // ---------------------------------------------------------
        var enrollQuery = _db.Set<StudentEnrollmentEntity>().AsNoTracking()
            .Where(e => e.AcademicYearId == request.Request.AcademicYearId);

        if (request.Request.ClassId.HasValue)
        {
            enrollQuery = enrollQuery.Where(e => e.ClassId == request.Request.ClassId.Value);
            if (request.Request.SectionId.HasValue)
                enrollQuery = enrollQuery.Where(e => e.SectionId == request.Request.SectionId.Value);
        }

        var studentIds = await enrollQuery.Select(e => e.StudentId).Distinct()
            .Take(MaxStudentsPerRequest + 1).ToListAsync(ct);
        if (studentIds.Count > MaxStudentsPerRequest)
            return Error.Validation($"Bulk generation is capped at {MaxStudentsPerRequest} students per request in Phase 1.");

        var attemptedPairs = studentIds.Count * requestedSequences.Count;

        if (studentIds.Count == 0)
            return Result.Success(new BulkGenerateResult(0, 0, 0, 0, Array.Empty<string>()));

        // ---------------------------------------------------------
        // BLOCK: Duplicates — keyed on PeriodSequenceNumber per
        // (student, sequence) pair. Manual / ADMISSION invoices have
        // a null sequence and never collide with periodic generation.
        // ---------------------------------------------------------
        var existing = await _db.Set<InvoiceEntity>().AsNoTracking()
            .Where(i => i.TenantId == tenantId && i.BranchId == branchId &&
                        i.AcademicYearId == request.Request.AcademicYearId &&
                        i.Status != InvoiceStatus.Cancelled &&
                        i.DeletedAt == null &&
                        i.PeriodSequenceNumber.HasValue &&
                        requestedSequences.Contains(i.PeriodSequenceNumber.Value) &&
                        studentIds.Contains(i.StudentId))
            .Select(i => new { i.StudentId, Seq = i.PeriodSequenceNumber!.Value })
            .ToListAsync(ct);
        var existingSet = existing
            .Select(x => (x.StudentId, x.Seq))
            .ToHashSet();
        // ---------------------------------------------------------
        // BLOCK: Sequential enforcement, batch-wide in ONE query.
        // All periods earlier than the highest requested sequence,
        // per student. Periods generated inside this run count too
        // (ascending processing makes that sound).
        // ---------------------------------------------------------
        var maxRequestedSequence = requestedSequences[^1];

        var generatedSequences = await _db.Set<InvoiceEntity>().AsNoTracking()
            .Where(i => i.TenantId == tenantId && i.BranchId == branchId &&
                        i.AcademicYearId == request.Request.AcademicYearId &&
                        i.Status != InvoiceStatus.Cancelled &&
                        i.DeletedAt == null &&
                        i.PeriodSequenceNumber.HasValue &&
                        i.PeriodSequenceNumber < maxRequestedSequence &&
                        studentIds.Contains(i.StudentId))
            .Select(i => new { i.StudentId, Seq = i.PeriodSequenceNumber!.Value })
            .ToListAsync(ct);

        var generatedByStudent = generatedSequences
            .GroupBy(x => x.StudentId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.Seq).ToHashSet());

        var skipped = 0;
        var generated = 0;
        var failed = 0;
        var errors = new List<string>();

        // ---------------------------------------------------------
        // BLOCK: Unit of work — the context owns the transaction
        // internally (begin / commit / rollback via the context),
        // exactly like the single handler. No 'await using var tx'.
        // ---------------------------------------------------------
        await _db.BeginTransactionAsync(ct);
        try
        {
            foreach (var sid in studentIds)
            {
                generatedByStudent.TryGetValue(sid, out var done);
                done ??= new HashSet<int>();

                foreach (var sequenceNumber in requestedSequences)
                {
                    // 1) Live invoice already exists for (student, period) -> skip.
                    if (existingSet.Contains((sid, sequenceNumber))) { skipped++; continue; }

                    // 2) All earlier periods must be invoiced for this student
                    //    (already in DB, or generated earlier in THIS run).
                    var missing = billingPeriods
                        .Where(p => p.SequenceNumber < sequenceNumber && !done.Contains(p.SequenceNumber))
                        .Select(p => p.SequenceNumber)
                        .OrderBy(s => s)
                        .ToList();

                    if (missing.Count > 0)
                    {
                        // Everything from this sequence onward is blocked for
                        // this student — count them failed once, move on.
                        var remaining = requestedSequences.Count - requestedSequences.IndexOf(sequenceNumber);
                        failed += remaining;
                        errors.Add(
                            $"Student {sid}: cannot generate period {sequenceNumber}; " +
                            $"missing earlier period(s) {string.Join(", ", missing)}.");
                        break;
                    }

                    // 3) Build the draft — period-aware pipeline, transport included.
                    var billingPeriod = periodBySequence[sequenceNumber];
                    var periodLabel = PeriodLabelBuilder.ForBillingPeriod(billingPeriod);

                    var draftResult = await InvoiceBuilder.BuildForPeriodAsync(
                        db: _db,
                        branchId: branchId,
                        studentId: sid,
                        academicYearId: request.Request.AcademicYearId,
                        academicYearStartDate: ayInfo.StartDate,
                        academicYearEndDate: ayInfo.EndDate,
                        billingPeriod: billingPeriod,
                        periodLabel: periodLabel,
                        generationDate: generationDate,
                        transportProvider: _transportProvider,
                        ct: ct);

                    if (draftResult.IsFailure)
                    {
                        failed++;
                        errors.Add($"Student {sid}, period {sequenceNumber}: {draftResult.Error!.ToString()}");
                        continue; // later seqs will trip the missing-period check on their own
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
                        // Billing period tracking.
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
                        // Header late-fee snapshots — frozen at invoice birth by
                        // the builder, same 7 as the single handler.
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

                    // Save inside the transaction so invoice.Id is assigned
                    // before the concession usage rows reference it.
                    _db.Set<InvoiceEntity>().Add(invoice);
                    await _db.SaveChangesAsync(ct);

                    // -----------------------------------------------------
                    // Concession usage tracking — one row per
                    // (allocation, invoice). The old bulk handler skipped
                    // this entirely; the single handler persists it, so
                    // bulk must too or concession dedup breaks silently.
                    // -----------------------------------------------------
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

                    done.Add(sequenceNumber);
                    generated++;
                }
            }

            await _db.CommitTransactionAsync(ct);
        }
        catch
        {
            await _db.RollbackTransactionAsync(CancellationToken.None);
            throw;
        }

        return Result.Success(new BulkGenerateResult(attemptedPairs, generated, skipped, failed, errors));
    }
}