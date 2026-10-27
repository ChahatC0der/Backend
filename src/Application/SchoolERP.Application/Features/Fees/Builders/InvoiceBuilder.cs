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
