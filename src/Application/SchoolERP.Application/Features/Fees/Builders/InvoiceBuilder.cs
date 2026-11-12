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

        var lineDrafts = new List<InvoiceLineDraft>();
        var usageDrafts = new List<ConcessionUsageDraft>();
        decimal totalAmount = 0m;
        decimal totalConcession = 0m;

        foreach (var fl in structure.Lines.Where(l => l.DeletedAt == null))
        {
            if (!feeHeads.TryGetValue(fl.FeeHeadId, out var head)) continue;

            var applicableConcessions = new List<ConcessionEntity>();
            foreach (var alloc in allocations)
            {
                if (!concessionsById.TryGetValue(alloc.ConcessionId, out var c)) continue;
                if (c.FeeHeadId.HasValue && c.FeeHeadId.Value != fl.FeeHeadId) continue;
                applicableConcessions.Add(c);
            }

            var concessionAmount = ConcessionCalculator.Calculate(fl.Amount, applicableConcessions);
            var netAmount = Money.Round(fl.Amount - concessionAmount);

            lineDrafts.Add(new InvoiceLineDraft
            {
                FeeHeadId = fl.FeeHeadId,
                FeeHeadNameSnapshot = head.Name,
                Amount = fl.Amount,
                ConcessionApplied = concessionAmount,
                NetAmount = netAmount,
                LateFeeApplicable = fl.LateFeeApplicable,
                LateFeeTypeSnapshot = fl.LateFeeType is null ? null : EnumMaps_JsonAdapter.LateFeeTypeMap[fl.LateFeeType.Value],
                LateFeeValueSnapshot = fl.LateFeeValue,
                LateFeeGraceDaysSnapshot = fl.LateFeeGraceDays,
                LateFeeCapPercentSnapshot = fl.LateFeeCapPercent,
                LateFeeSlabConfigSnapshot = fl.LateFeeSlabConfig,
                LateFeeCapBase = netAmount,
            });

            totalAmount += fl.Amount;
            totalConcession += concessionAmount;

            if (concessionAmount > 0 && applicableConcessions.Count > 0)
            {
                foreach (var alloc in allocations)
                {
                    if (!concessionsById.ContainsKey(alloc.ConcessionId)) continue;
                    var c = concessionsById[alloc.ConcessionId];
                    if (c.FeeHeadId.HasValue && c.FeeHeadId.Value != fl.FeeHeadId) continue;
                    var share = Money.Round(concessionAmount / applicableConcessions.Count);
                    if (share > 0)
                        usageDrafts.Add(new ConcessionUsageDraft
                        {
                            ConcessionAllocationId = alloc.Id,
                            AppliedAmount = share,
                        });
                }
            }
        }

        if (lineDrafts.Count == 0)
            return Error.Validation($"Fee structure {structure.Id} has no active lines.");

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
    /// 4. Apply existing concession calculation.
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
        DateTime dueDate,
        // Optional cross-module provider.
        // When supplied, transport charges are included in the draft.
        ITransportFeeQuoteProvider? transportProvider = null,
        CancellationToken ct = default)
    {
        // ---------------------------------------------------------
        // BLOCK 1: Validate the generation input.
        // The builder must receive a complete and valid billing period.
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


        // BLOCK: Retrieve optional transport quote.
        //
        // The Infrastructure provider resolves the student's active
        // transport assignment and returns its effective fee.
        //
        // A null quote means the student has no configured transport
        // charge for this Academic Year.
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
        //
        // The loader retrieves:
        // Student, Enrollment, FeeStructure, FeeHeads
        // and applicable concession records.
        //
        // Database queries are not duplicated in this builder.
        // ---------------------------------------------------------


        var contextResult =
            await InvoiceGenerationContextLoader.LoadAsync(
                db,
                branchId,
                studentId,
                academicYearId,
                billingPeriod,
                generationDate,

                // Ask the loader to include the transport FeeHead
                // in addition to regular structure FeeHeads.
                transportQuote?.TransportFeeHeadCode,

                ct);


        if (contextResult.IsFailure)
        {
            return contextResult.Error!;
        }

        var context = contextResult.Value;

        // ---------------------------------------------------------
        // BLOCK 3: Prepare invoice calculation collections.
        //
        // Line drafts contain the frozen fee snapshots.
        // Usage totals are aggregated by concession allocation ID.
        //
        // One concession allocation should produce at most one
        // usage record per generated invoice.
        // ---------------------------------------------------------

        var lineDrafts = new List<InvoiceLineDraft>();

        var concessionUsageTotals = new Dictionary<long, decimal>();

        decimal totalAmount = 0m;
        decimal totalConcession = 0m;

        // ---------------------------------------------------------
        // BLOCK 4: Process active FeeStructureLines.
        //
        // Each fee head is evaluated independently.
        //"OneTime lines are included exactly once per student + AY (see BLOCK 5 rule)."
        // ---------------------------------------------------------

        foreach (var feeLine in context.FeeStructure.Lines
                     .Where(line => line.DeletedAt == null))
        {
            // Every structure line must have a corresponding FeeHead.
            // Missing data must not silently reduce invoice amounts.
            if (!context.FeeHeads.TryGetValue(
                    feeLine.FeeHeadId,
                    out var feeHead))
            {
                return Error.Validation(
                    $"FeeHead {feeLine.FeeHeadId} was not found " +
                    $"for FeeStructure {context.FeeStructure.Id}.");
            }

            // -----------------------------------------------------
            // BLOCK 5: Calculate fee recurrence.
            //
            // OneTime lines: "exactly-once among ACTIVE invoices" rule —
            // billed (x1) only if no other non-cancelled invoice of this
            // student + AY already contains a line sourced from this
            // FeeStructureLine (matched by source line id, NOT FeeHeadId —
            // same head ki recurring + one-time dono lines ho sakti hain).
            //
            // One rule, saare cases covered:
            //   Normal student       -> pehli generated invoice me
            //   Mid-year joiner      -> uski first eligible invoice me
            //   Line baad me add hui -> next generated invoice me
            //   Host cancel + regen  -> wapas wahi period me
            //                           (cancelled rows count nahi hote)
            // -----------------------------------------------------

            int occurrenceCount;

            if (feeLine.Frequency == FeeFrequency.OneTime)
            {
                if (context.AlreadyBilledSourceLineIds.Contains(feeLine.Id))
                {
                    continue;   // already billed in an active invoice — skip
                }

                occurrenceCount = 1;
            }
            else
            {
                occurrenceCount =
                    FeeHeadFrequencyPolicy.GetOccurrenceCount(
                        feeLine.Frequency,
                        academicYearStartDate,
                        academicYearEndDate,
                        billingPeriod);

                if (occurrenceCount == 0)
                {
                    continue;
                }
            }

            // -----------------------------------------------------
            // BLOCK 6: Calculate the gross charge.
            //
            // Amount is the configured charge per occurrence.
            // Multiply by the number of occurrences in this invoice.
            //
            // ProrationCalculator is intentionally NOT called here.
            // -----------------------------------------------------

            var lineAmount = Money.Round(
                feeLine.Amount * occurrenceCount);

            if (lineAmount <= 0m)
            {
                continue;
            }

            // -----------------------------------------------------
            // BLOCK 7: Resolve applicable concession allocations.
            //
            // A concession can apply to:
            // - All fee heads, when FeeHeadId is null.
            // - One specific FeeHead, when FeeHeadId is populated.
            //
            // Only allocations whose concession definition was loaded
            // as applicable by the context loader are considered.
            // -----------------------------------------------------

            var applicableAllocations =
                new List<(ConcessionAllocationEntity Allocation,
                          ConcessionEntity Concession)>();

            foreach (var allocation in context.ConcessionAllocations)
            {
                if (!context.Concessions.TryGetValue(
                        allocation.ConcessionId,
                        out var concession))
                {
                    continue;
                }

                // Skip concessions assigned to a different FeeHead.
                if (concession.FeeHeadId.HasValue &&
                    concession.FeeHeadId.Value != feeLine.FeeHeadId)
                {
                    continue;
                }

                applicableAllocations.Add((allocation, concession));
            }

            // -----------------------------------------------------
            // BLOCK 8: Calculate concession and net payable.
            //
            // Reuse the existing ConcessionCalculator.
            // Percentage and fixed concession stacking behaviour
            // remains owned by that existing class.
            // -----------------------------------------------------

            var concessionAmount = ConcessionCalculator.Calculate(
                lineAmount,
                applicableAllocations.Select(x => x.Concession));

            var netAmount = Money.Round(
                lineAmount - concessionAmount);

            // -----------------------------------------------------
            // BLOCK 9: Construct the invoice line snapshot.
            //
            // Values are copied from the FeeStructureLine and FeeHead.
            // Later changes to the FeeStructure must not change
            // an already generated invoice line.
            // -----------------------------------------------------

            lineDrafts.Add(new InvoiceLineDraft
            {
                FeeHeadId = feeLine.FeeHeadId,

                FeeHeadNameSnapshot = feeHead.Name,
                SourceFeeStructureLineId = feeLine.Id,

                Amount = lineAmount,

                ConcessionApplied = concessionAmount,

                NetAmount = netAmount,

                LateFeeApplicable = feeLine.LateFeeApplicable,

                LateFeeTypeSnapshot =
                    feeLine.LateFeeType is null
                        ? null
                        : EnumMaps_JsonAdapter.LateFeeTypeMap[
                            feeLine.LateFeeType.Value],

                LateFeeValueSnapshot = feeLine.LateFeeValue,

                LateFeeGraceDaysSnapshot = feeLine.LateFeeGraceDays,

                LateFeeCapPercentSnapshot = feeLine.LateFeeCapPercent,

                LateFeeSlabConfigSnapshot = feeLine.LateFeeSlabConfig,

                LateFeeCapBase = netAmount
            });

            // -----------------------------------------------------
            // BLOCK 10: Update invoice totals.
            //
            // TotalAmount contains gross charges.
            // ConcessionAmount contains applied concessions.
            // NetPayable is calculated after all lines are processed.
            // -----------------------------------------------------

            totalAmount += lineAmount;
            totalConcession += concessionAmount;

            // -----------------------------------------------------
            // BLOCK 11: Aggregate concession usage.
            //
            // Current allocation policy distributes the calculated
            // concession evenly across applicable allocations.
            //
            // Any rounding remainder is assigned to the last allocation
            // so total usage exactly matches the applied concession.
            //
            // Usage is aggregated by allocation ID because the
            // existing ConcessionUsage model records allocation + invoice,
            // not allocation + invoice + line.
            // -----------------------------------------------------

            if (concessionAmount > 0m &&
                applicableAllocations.Count > 0)
            {
                var remainingAmount = concessionAmount;

                var roundedShare = Money.Round(
                    concessionAmount / applicableAllocations.Count);

                for (var index = 0;
                     index < applicableAllocations.Count;
                     index++)
                {
                    var allocation =
                        applicableAllocations[index].Allocation;

                    var isLast =
                        index == applicableAllocations.Count - 1;

                    var share = isLast
                        ? remainingAmount
                        : Money.Min(roundedShare, remainingAmount);

                    if (share <= 0m)
                    {
                        continue;
                    }

                    concessionUsageTotals.TryGetValue(
                        allocation.Id,
                        out var existingAmount);

                    concessionUsageTotals[allocation.Id] =
                        Money.Round(existingAmount + share);

                    remainingAmount =
                        Money.Round(remainingAmount - share);
                }
            }
        }


        // ---------------------------------------------------------
        // BLOCK: TRANSPORT FEE INTEGRATION
        //
        // Transport quote and FeeHead are handled separately
        // from regular FeeStructureLines.
        //
        // The TransportFeeLineBuilder decides:
        // - Whether the transport frequency applies.
        // - How many occurrences belong to this billing period.
        // - What amount should be charged.
        // ---------------------------------------------------------

        if (transportQuote is not null)
        {
            // Resolve the transport FeeHead from the context.
            // Database access stays inside InvoiceGenerationContextLoader.
            var transportFeeHead = context.FeeHeads.Values
                .FirstOrDefault(h =>
                    string.Equals(
                        h.Code,
                        transportQuote.TransportFeeHeadCode,
                        StringComparison.OrdinalIgnoreCase));

            // A configured transport quote without a valid active
            // FeeHead must not be silently ignored.
            if (transportFeeHead is null ||
                transportFeeHead.Status != FeeHeadStatus.Active)
            {
                return Error.Validation(
                    $"Active transport FeeHead " +
                    $"'{transportQuote.TransportFeeHeadCode}' " +
                    "was not found for the current branch.");
            }

            // Prevent accidental double billing when the same
            // transport FeeHead is already included in the structure.
            if (lineDrafts.Any(l =>
                    l.FeeHeadId == transportFeeHead.Id))
            {
                return Error.Validation(
                    "Transport FeeHead is already included in the " +
                    "FeeStructure. It cannot also be added through " +
                    "the transport quote.");
            }

            // Delegate recurrence and invoice-line construction
            // to the dedicated transport builder.
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

            // Add the transport line only when it is applicable
            // to this billing period.
            if (transportLineResult.Value is { } transportLine)
            {
                lineDrafts.Add(transportLine);

                // Transport does not receive concession in this flow,
                // so only gross amount is added here.
                totalAmount += transportLine.Amount;
            }
        }

        // ---------------------------------------------------------
        // BLOCK: FINAL LINE VALIDATION
        //
        // Validate after both regular and transport lines have
        // been processed.
        // ---------------------------------------------------------

        if (lineDrafts.Count == 0)
        {
            return Error.Validation(
                $"No applicable fee lines found for billing period " +
                $"'{periodLabel}'.");
        }


        // ---------------------------------------------------------
        // BLOCK 12: Validate the final line collection.
        //
        // A recurring invoice must have at least one applicable
        // recurring FeeStructureLine.
        //
        //"OneTime lines land in the first period where they are active and not yet billed."
        // ---------------------------------------------------------

        if (lineDrafts.Count == 0)
        {
            return Error.Validation(
                $"FeeStructure {context.FeeStructure.Id} has no " +
                $"applicable recurring fee lines for period '{periodLabel}'.");
        }

        // ---------------------------------------------------------
        // BLOCK 13: Construct concession usage drafts.
        //
        // One row per concession allocation.
        // Amounts are already aggregated across all invoice lines.
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
        // BLOCK 14: Construct the existing InvoiceDraft.
        //
        // The existing command handlers will continue to persist
        // InvoiceDraft and its InvoiceLineDraft collection.
        //
        // No Invoice entity or database mutation is performed here.
        // ---------------------------------------------------------

        var draft = new InvoiceDraft
        {
            StudentId = context.Student.Id,

            AcademicYearId = academicYearId,

            ClassId = context.Enrollment.ClassId,

            SectionId = context.Enrollment.SectionId,

            FeeStructureId = context.FeeStructure.Id,

            PeriodLabel = periodLabel.Trim(),

            GenerationDate = generationDate,

            DueDate = dueDate,

            TotalAmount = Money.Round(totalAmount),

            ConcessionAmount = Money.Round(totalConcession),

            NetPayable = Money.Round(totalAmount - totalConcession),

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
}
