using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Fees.Enums;
using SchoolERP.Domain.Fees.ValueObjects;
using SchoolERP.Domain.Shared.Results;
using FeeStructureEntity = SchoolERP.Domain.Fees.Entities.FeeStructure;
using FeeStructureLineEntity = SchoolERP.Domain.Fees.Entities.FeeStructureLine;
using FeeHeadEntity = SchoolERP.Domain.Fees.Entities.FeeHead;
using ConcessionEntity = SchoolERP.Domain.Fees.Entities.Concession;
using ConcessionAllocationEntity = SchoolERP.Domain.Fees.Entities.ConcessionAllocation;
using StudentEntity = SchoolERP.Domain.Student.Entities.Student;
using StudentEnrollmentEntity = SchoolERP.Domain.Student.Entities.StudentEnrollment;
using StudentFeeAssignmentEntity = SchoolERP.Domain.Fees.Entities.StudentFeeAssignment;

namespace SchoolERP.Application.Features.Fees.Builders;

/// <summary>
/// Pure in-memory invoice construction. No DB writes.
/// Optionally appends a transport fee line (Phase 6) when transportProvider is supplied
/// and the student has an active transport assignment with a configured fee.
///
/// Concession flow: builder = WHO (load + scope + dedup),
/// ConcessionEngine = HOW MUCH (partition → per-line → pool → combine),
/// ConcessionCalculator = ATOM (ek line, ek list).
/// </summary>
public static class InvoiceBuilder
{
    public static async Task<Result<InvoiceDraft>> BuildAsync(
        IApplicationDbContext db,
        long studentId,
        long academicYearId,
        string periodLabel,
        DateTime generationDate,
        DateTime dueDate,
        ITransportFeeQuoteProvider? transportProvider = null,
        CancellationToken ct = default)
    {
        var studentExists = await db.Set<StudentEntity>().AsNoTracking()
            .AnyAsync(s => s.Id == studentId, ct);
        if (!studentExists) return Error.NotFound("Student", studentId.ToString());

        var enrollment = await db.Set<StudentEnrollmentEntity>().AsNoTracking()
            .Where(e => e.StudentId == studentId && e.AcademicYearId == academicYearId)
            .OrderByDescending(e => e.Id)
            .Select(e => new { e.ClassId, e.SectionId })
            .FirstOrDefaultAsync(ct);
        if (enrollment is null)
            return Error.Validation($"Student {studentId} has no enrollment for academic year {academicYearId}.");

        var structure = await ResolveStructureAsync(db, studentId, academicYearId,
            enrollment.ClassId, enrollment.SectionId, ct);
        if (structure is null)
            return Error.Validation($"No active fee structure found for student {studentId}.");

        var feeHeadIds = structure.Lines.Where(l => l.DeletedAt == null)
            .Select(l => l.FeeHeadId).Distinct().ToList();
        var feeHeads = await db.Set<FeeHeadEntity>().AsNoTracking()
            .Where(h => feeHeadIds.Contains(h.Id))
            .ToDictionaryAsync(h => h.Id, h => h, ct);

        var allocations = await db.Set<ConcessionAllocationEntity>().AsNoTracking()
            .Where(a => a.StudentId == studentId && a.AcademicYearId == academicYearId
                     && a.Status == ConcessionAllocationStatus.Approved)
            .ToListAsync(ct);

        var concessionIds = allocations.Select(a => a.ConcessionId).Distinct().ToList();
        var concessions = await db.Set<ConcessionEntity>().AsNoTracking()
            .Where(c => concessionIds.Contains(c.Id) && c.DeletedAt == null
                     && (c.StartDate == null || c.StartDate <= generationDate)
                     && (c.EndDate == null || c.EndDate >= generationDate))
            .ToListAsync(ct);

        var concessionsById = concessions.ToDictionary(c => c.Id);

        // =========================================================
        // SECTION 1: LOAD — done above (scope + status + date window)
        // =========================================================

        // =========================================================
        // SECTION 2: CALCULATE — pure, ek engine call
        // =========================================================

        // PASS A: lines collect (deterministic order — pool last-line paisa-absorb depend karta hai)
        var calcLines = new List<(FeeStructureLineEntity Line, FeeHeadEntity Head, decimal Gross)>();

        foreach (var fl in structure.Lines
                     .Where(l => l.DeletedAt == null)
                     .OrderBy(l => l.Id))
        {
            if (!feeHeads.TryGetValue(fl.FeeHeadId, out var head)) continue;
            calcLines.Add((fl, head, fl.Amount));
        }

        if (calcLines.Count == 0)
            return Error.Validation($"Fee structure {structure.Id} has no active lines.");

        // WHO: dedup by ConcessionId — double-approval safety net
        var activeConcessions = allocations
            .Where(a => concessionsById.ContainsKey(a.ConcessionId))
            .Select(a => concessionsById[a.ConcessionId])
            .DistinctBy(c => c.Id)
            .ToList();

        var concessionResult = ConcessionEngine.Evaluate(
            calcLines.Select(x => new ConcessionLineInput(x.Line.Id, x.Line.FeeHeadId, x.Gross)).ToList(),
            activeConcessions);

        // usage attribution ke liye allocation pairs (concession-deduped)
        var usagePairs = allocations
            .Where(a => concessionsById.ContainsKey(a.ConcessionId))
            .GroupBy(a => a.ConcessionId)
            .Select(g => (Allocation: g.First(), Concession: concessionsById[g.Key]))
            .ToList();

        // =========================================================
        // SECTION 3: FREEZE — zip by index: calcLines[i] ↔ Lines[i]
        // =========================================================

        var lineDrafts = new List<InvoiceLineDraft>(calcLines.Count);

        // usage AB aggregate hoga (allocation+invoice unique) — bug fix
        var usageTotals = new Dictionary<long, decimal>();

        for (int i = 0; i < calcLines.Count; i++)
        {
            var (fl, head, gross) = calcLines[i];
            var cr = concessionResult.Lines[i];

            lineDrafts.Add(new InvoiceLineDraft
            {
                FeeHeadId = fl.FeeHeadId,
                FeeHeadNameSnapshot = head.Name,
                Amount = gross,
                ConcessionApplied = cr.ConcessionAmount,
                NetAmount = cr.NetAmount,
                LateFeeApplicable = fl.LateFeeApplicable,
                LateFeeTypeSnapshot = fl.LateFeeType is null ? null : EnumMaps_JsonAdapter.LateFeeTypeMap[fl.LateFeeType.Value],
                LateFeeValueSnapshot = fl.LateFeeValue,
                LateFeeGraceDaysSnapshot = fl.LateFeeGraceDays,
                LateFeeCapPercentSnapshot = fl.LateFeeCapPercent,
                LateFeeSlabConfigSnapshot = fl.LateFeeSlabConfig,
                LateFeeCapBase = cr.NetAmount,
            });

            // Even-split + last-absorb (weighted split deferred to E2E)
            var linePairs = usagePairs
                .Where(p => p.Concession.FeeHeadId is null || p.Concession.FeeHeadId == fl.FeeHeadId)
                .ToList();

            if (cr.ConcessionAmount > 0m && linePairs.Count > 0)
            {
                var remaining = cr.ConcessionAmount;
                var share = Money.Round(cr.ConcessionAmount / linePairs.Count);

                for (var j = 0; j < linePairs.Count; j++)
                {
                    var isLast = j == linePairs.Count - 1;
                    var s = isLast ? remaining : Money.Min(share, remaining);
                    if (s <= 0m) continue;

                    usageTotals.TryGetValue(linePairs[j].Allocation.Id, out var existing);
                    usageTotals[linePairs[j].Allocation.Id] = Money.Round(existing + s);
                    remaining = Money.Round(remaining - s);
                }
            }
        }

        decimal totalAmount = concessionResult.TotalGross;
        decimal totalConcession = concessionResult.TotalConcession;

        var usageDrafts = usageTotals
            .Where(x => x.Value > 0m)
            .Select(x => new ConcessionUsageDraft
            {
                ConcessionAllocationId = x.Key,
                AppliedAmount = x.Value
            })
            .ToList();

        // ---- Transport fee integration (Phase 6) ----
        if (transportProvider is not null)
        {
            var quote = await transportProvider.GetQuoteAsync(studentId, academicYearId, ct);
            if (quote is not null && quote.Amount > 0)
            {
                var transportHead = await db.Set<FeeHeadEntity>().AsNoTracking()
                    .FirstOrDefaultAsync(h => h.Code == quote.TransportFeeHeadCode
                        && h.DeletedAt == null
                        && h.Status == FeeHeadStatus.Active, ct);

                if (transportHead is not null)
                {
                    lineDrafts.Add(new InvoiceLineDraft
                    {
                        FeeHeadId = transportHead.Id,
                        FeeHeadNameSnapshot = transportHead.Name,
                        Amount = quote.Amount,
                        ConcessionApplied = 0m,
                        NetAmount = quote.Amount,
                        LateFeeApplicable = false,
                        LateFeeTypeSnapshot = null,
                        LateFeeValueSnapshot = null,
                        LateFeeGraceDaysSnapshot = null,
                        LateFeeCapPercentSnapshot = null,
                        LateFeeSlabConfigSnapshot = null,
                        LateFeeCapBase = quote.Amount,
                    });
                    totalAmount += quote.Amount;
                }
            }
        }
        var structureHeader = structure;
        var draft = new InvoiceDraft
        {
            StudentId = studentId,
            AcademicYearId = academicYearId,
            ClassId = enrollment.ClassId,
            SectionId = enrollment.SectionId,
            FeeStructureId = structure.Id,
            PeriodLabel = periodLabel,
            GenerationDate = generationDate,
            DueDate = dueDate,
            TotalAmount = Money.Round(totalAmount),
            ConcessionAmount = Money.Round(totalConcession),

            NetPayable = Money.Round(totalAmount - totalConcession),

            // ---- SECTION 4: FREEZE — invoice-header late fee snapshot ----
            LateFeeModeSnapshot = ResolveHeaderLateFeeMode(structure),
            InvoiceLateFeeTypeSnapshot = structureHeader.InvoiceLateFeeType,
            InvoiceLateFeeValueSnapshot = structureHeader.InvoiceLateFeeValue,
            InvoiceLateFeeGraceDaysSnapshot = structureHeader.InvoiceLateFeeGraceDays,
            InvoiceLateFeeCapPercentSnapshot = structureHeader.InvoiceLateFeeCapPercent,
            InvoiceLateFeeSlabConfigSnapshot = structureHeader.InvoiceLateFeeSlabConfig,
            InvoiceLateFeeCapBase = Money.Round(totalAmount - totalConcession),
            Lines = lineDrafts,
            ConcessionUsages = usageDrafts,
        };

        return Result.Success(draft);
    }


    /// <summary>
    /// Builds an invoice draft for an explicitly calculated billing period.
    ///
    /// This is the new period-aware generation pipeline.
    ///
    /// Responsibilities:
    /// 1. Load invoice generation context.
    /// 2. Select FeeStructureLines applicable to this period.
    /// 3. Calculate recurring fee amounts.
    /// 4. Apply concession math via ConcessionEngine.
    /// 5. Aggregate concession usage.
    /// 6. Construct the existing InvoiceDraft.
    ///
    /// Database loading is delegated to InvoiceGenerationContextLoader.
    /// Fee recurrence is delegated to FeeHeadFrequencyPolicy.
    /// Student eligibility is handled during context loading.
    ///
    /// This method does not save invoices.
    /// Proration is intentionally not applied.
    /// </summary>
    public static async Task<Result<InvoiceDraft>> BuildForPeriodAsync(
        IApplicationDbContext db,
        Guid branchId,
        long studentId,
        long academicYearId,
        DateOnly academicYearStartDate,
        DateOnly academicYearEndDate,
        BillingPeriod billingPeriod,
        string periodLabel,
        DateTime generationDate,
        ITransportFeeQuoteProvider? transportProvider = null,
        CancellationToken ct = default)
    {
        // ---------------------------------------------------------
        // BLOCK 1: Validate the generation input.
        // ---------------------------------------------------------

        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(billingPeriod);

        if (string.IsNullOrWhiteSpace(periodLabel))
        {
            return Error.Validation("Invoice PeriodLabel is required.");
        }

        if (academicYearEndDate < academicYearStartDate)
        {
            return Error.Validation(
                "Academic Year end date cannot be earlier than start date.");
        }

        // ---------------------------------------------------------
        // BLOCK: Retrieve optional transport quote.
        // ---------------------------------------------------------

        TransportFeeQuote? transportQuote = null;

        if (transportProvider is not null)
        {
            transportQuote = await transportProvider.GetQuoteAsync(
                studentId,
                academicYearId,
                ct);
        }

        // ---------------------------------------------------------
        // BLOCK 2: Load the generation context.
        // ---------------------------------------------------------

        var contextResult =
            await InvoiceGenerationContextLoader.LoadAsync(
                db,
                branchId,
                studentId,
                academicYearId,
                billingPeriod,
                generationDate,
                transportQuote?.TransportFeeHeadCode,
                ct);

        if (contextResult.IsFailure)
        {
            return contextResult.Error!;
        }

        var context = contextResult.Value;

        // ---------------------------------------------------------
        // BLOCK 3: Prepare invoice calculation collections.
        // ---------------------------------------------------------

        var lineDrafts = new List<InvoiceLineDraft>();
        var concessionUsageTotals = new Dictionary<long, decimal>();

        // =========================================================
        // SECTION 2: CALCULATE — pure, ek engine call
        // =========================================================

        // PASS A: BLOCK 4 + 5 + 6 — recurrence + gross collect
        //         (OneTime exactly-once + recurring occurrence count)
        //         Deterministic OrderBy — pool last-line paisa-absorb depend karta hai.

        var calcLines = new List<(FeeStructureLineEntity Line, FeeHeadEntity Head, decimal Gross)>();

        foreach (var feeLine in context.FeeStructure.Lines
                     .Where(line => line.DeletedAt == null)
                     .OrderBy(line => line.Id))
        {
            if (!context.FeeHeads.TryGetValue(feeLine.FeeHeadId, out var feeHead))
            {
                return Error.Validation(
                    $"FeeHead {feeLine.FeeHeadId} was not found " +
                    $"for FeeStructure {context.FeeStructure.Id}.");
            }

            // BLOCK 5: recurrence — OneTime exactly-once among active invoices
            int occurrenceCount;

            if (feeLine.Frequency == FeeFrequency.OneTime)
            {
                if (context.AlreadyBilledSourceLineIds.Contains(feeLine.Id))
                    continue;
                occurrenceCount = 1;
            }
            else
            {
                occurrenceCount = FeeHeadFrequencyPolicy.GetOccurrenceCount(
                    feeLine.Frequency,
                    academicYearStartDate,
                    academicYearEndDate,
                    billingPeriod);
                if (occurrenceCount == 0) continue;
            }

            // BLOCK 6: gross charge (no proration)
            var lineAmount = Money.Round(feeLine.Amount * occurrenceCount);
            if (lineAmount <= 0m) continue;

            calcLines.Add((feeLine, feeHead, lineAmount));
        }

        // BLOCK 7 + 8: WHO (dedup) + HOW MUCH (engine)
        var activeConcessions = context.ConcessionAllocations
            .Where(a => context.Concessions.ContainsKey(a.ConcessionId))
            .Select(a => context.Concessions[a.ConcessionId])
            .DistinctBy(c => c.Id)
            .ToList();

        var concessionResult = ConcessionEngine.Evaluate(
            calcLines.Select(x => new ConcessionLineInput(x.Line.Id, x.Line.FeeHeadId, x.Gross)).ToList(),
            activeConcessions);

        var usagePairs = context.ConcessionAllocations
            .Where(a => context.Concessions.ContainsKey(a.ConcessionId))
            .GroupBy(a => a.ConcessionId)
            .Select(g => (Allocation: g.First(), Concession: context.Concessions[g.Key]))
            .ToList();

        // =========================================================
        // SECTION 3: FREEZE — zip by index: calcLines[i] ↔ Lines[i]
        // =========================================================

        for (int i = 0; i < calcLines.Count; i++)
        {
            var (feeLine, feeHead, lineAmount) = calcLines[i];
            var cr = concessionResult.Lines[i];

            // BLOCK 9: line snapshot
            lineDrafts.Add(new InvoiceLineDraft
            {
                FeeHeadId = feeLine.FeeHeadId,
                FeeHeadNameSnapshot = feeHead.Name,
                SourceFeeStructureLineId = feeLine.Id,
                Amount = lineAmount,
                ConcessionApplied = cr.ConcessionAmount,
                NetAmount = cr.NetAmount,
                LateFeeApplicable = feeLine.LateFeeApplicable,
                LateFeeTypeSnapshot = feeLine.LateFeeType is null
                    ? null
                    : EnumMaps_JsonAdapter.LateFeeTypeMap[feeLine.LateFeeType.Value],
                LateFeeValueSnapshot = feeLine.LateFeeValue,
                LateFeeGraceDaysSnapshot = feeLine.LateFeeGraceDays,
                LateFeeCapPercentSnapshot = feeLine.LateFeeCapPercent,
                LateFeeSlabConfigSnapshot = feeLine.LateFeeSlabConfig,
                LateFeeCapBase = cr.NetAmount,
            });

            // BLOCK 11: usage aggregation — even-split + last-absorb
            var linePairs = usagePairs
                .Where(p => p.Concession.FeeHeadId is null || p.Concession.FeeHeadId == feeLine.FeeHeadId)
                .ToList();

            if (cr.ConcessionAmount > 0m && linePairs.Count > 0)
            {
                var remainingAmount = cr.ConcessionAmount;
                var roundedShare = Money.Round(cr.ConcessionAmount / linePairs.Count);

                for (var index = 0; index < linePairs.Count; index++)
                {
                    var isLast = index == linePairs.Count - 1;
                    var share = isLast ? remainingAmount : Money.Min(roundedShare, remainingAmount);
                    if (share <= 0m) continue;

                    var allocationId = linePairs[index].Allocation.Id;
                    concessionUsageTotals.TryGetValue(allocationId, out var existingAmount);
                    concessionUsageTotals[allocationId] = Money.Round(existingAmount + share);
                    remainingAmount = Money.Round(remainingAmount - share);
                }
            }
        }

        decimal totalAmount = concessionResult.TotalGross;
        decimal totalConcession = concessionResult.TotalConcession;

        // ---------------------------------------------------------
        // BLOCK: TRANSPORT FEE INTEGRATION
        // ---------------------------------------------------------

        if (transportQuote is not null)
        {
            var transportFeeHead = context.FeeHeads.Values
                .FirstOrDefault(h =>
                    string.Equals(
                        h.Code,
                        transportQuote.TransportFeeHeadCode,
                        StringComparison.OrdinalIgnoreCase));

            if (transportFeeHead is null ||
                transportFeeHead.Status != FeeHeadStatus.Active)
            {
                return Error.Validation(
                    $"Active transport FeeHead " +
                    $"'{transportQuote.TransportFeeHeadCode}' " +
                    "was not found for the current branch.");
            }

            if (lineDrafts.Any(l => l.FeeHeadId == transportFeeHead.Id))
            {
                return Error.Validation(
                    "Transport FeeHead is already included in the " +
                    "FeeStructure. It cannot also be added through " +
                    "the transport quote.");
            }

            var transportLineResult = TransportFeeLineBuilder.Build(
                transportQuote,
                transportFeeHead,
                academicYearStartDate,
                academicYearEndDate,
                billingPeriod);

            if (transportLineResult.IsFailure)
            {
                return transportLineResult.Error!;
            }

            if (transportLineResult.Value is { } transportLine)
            {
                lineDrafts.Add(transportLine);
                totalAmount += transportLine.Amount;
            }
        }

        // ---------------------------------------------------------
        // BLOCK: FINAL LINE VALIDATION (single check — duplicate removed)
        // ---------------------------------------------------------

        if (lineDrafts.Count == 0)
        {
            return Error.Validation(
                $"No applicable fee lines found for billing period " +
                $"'{periodLabel}'.");
        }

        // ---------------------------------------------------------
        // BLOCK 13: Construct concession usage drafts.
        // ---------------------------------------------------------

        var usageDrafts = concessionUsageTotals
            .Where(x => x.Value > 0m)
            .Select(x => new ConcessionUsageDraft
            {
                ConcessionAllocationId = x.Key,
                AppliedAmount = x.Value
            })
            .ToList();

        // ---------------------------------------------------------
        // BLOCK 14: Construct the InvoiceDraft.
        // ---------------------------------------------------------

        var netPayable = Money.Round(totalAmount - totalConcession);
        var structureHeader = context.FeeStructure;

        var draft = new InvoiceDraft
        {
            StudentId = context.Student.Id,
            AcademicYearId = academicYearId,
            ClassId = context.Enrollment.ClassId,
            SectionId = context.Enrollment.SectionId,
            FeeStructureId = structureHeader.Id,
            PeriodLabel = periodLabel.Trim(),
            GenerationDate = generationDate,
            DueDate = billingPeriod.EndDate.ToDateTime(TimeOnly.MinValue),
            TotalAmount = Money.Round(totalAmount),
            ConcessionAmount = Money.Round(totalConcession),
            NetPayable = netPayable,

            // ---- SECTION 4: FREEZE — invoice-header late fee snapshot ----
            LateFeeModeSnapshot = ResolveHeaderLateFeeMode(structureHeader),
            InvoiceLateFeeTypeSnapshot = structureHeader.InvoiceLateFeeType,
            InvoiceLateFeeValueSnapshot = structureHeader.InvoiceLateFeeValue,
            InvoiceLateFeeGraceDaysSnapshot = structureHeader.InvoiceLateFeeGraceDays,
            InvoiceLateFeeCapPercentSnapshot = structureHeader.InvoiceLateFeeCapPercent,
            InvoiceLateFeeSlabConfigSnapshot = structureHeader.InvoiceLateFeeSlabConfig,
            InvoiceLateFeeCapBase = netPayable,

            Lines = lineDrafts,
            ConcessionUsages = usageDrafts
        };

        return Result.Success(draft);
    }

    private static async Task<FeeStructureEntity?> ResolveStructureAsync(
        IApplicationDbContext db, long studentId, long academicYearId,
        long classId, long? sectionId, CancellationToken ct)
    {
        var overrideId = await db.Set<StudentFeeAssignmentEntity>().AsNoTracking()
            .Where(a => a.StudentId == studentId && a.AcademicYearId == academicYearId && a.IsActive)
            .Select(a => (long?)a.FeeStructureId)
            .FirstOrDefaultAsync(ct);

        if (overrideId.HasValue)
        {
            var s = await db.Set<FeeStructureEntity>().AsNoTracking().Include(x => x.Lines)
                .FirstOrDefaultAsync(x => x.Id == overrideId.Value
                    && x.DeletedAt == null && x.Status == FeeStructureStatus.Active, ct);
            if (s is not null) return s;
        }

        var candidates = await db.Set<FeeStructureEntity>().AsNoTracking().Include(x => x.Lines)
            .Where(x => x.AcademicYearId == academicYearId
                     && x.DeletedAt == null && x.Status == FeeStructureStatus.Active)
            .ToListAsync(ct);

        var bySection = candidates.FirstOrDefault(x => x.ClassId == classId && x.SectionId == sectionId);
        if (bySection is not null) return bySection;

        var byClass = candidates.FirstOrDefault(x => x.ClassId == classId && x.SectionId == null);
        if (byClass is not null) return byClass;

        var byGroup = candidates.FirstOrDefault(x => x.ClassGroupId != null && x.ClassId == null && x.SectionId == null);
        return byGroup;
    }

    /// <summary>
    /// Resolves the invoice-header late fee mode string frozen onto the invoice.
    /// FeeStructure.LateFeeMode is a NON-NULLABLE enum — the header always
    /// carries an explicit mode. No HasValue check, no lines-derived fallback:
    /// what the school configured is what gets frozen onto every invoice.
    /// </summary>
    private static string ResolveHeaderLateFeeMode(FeeStructureEntity structure)
    {
        return structure.LateFeeMode switch
        {
            LateFeeMode.None => "none",
            LateFeeMode.PerHead => "per_head",
            LateFeeMode.WholeInvoice => "whole_invoice",
            _ => "per_head",
        };
    }
}