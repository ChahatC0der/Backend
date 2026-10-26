using Microsoft.EntityFrameworkCore;
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

namespace SchoolERP.Application.Features.Fees.Builders;

/// <summary>
/// Pure in-memory invoice construction — no DB writes. Called by GenerateStudentInvoice,
/// BulkGenerateInvoices, RegenerateInvoice. Freezes all snapshots on the draft.
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
        CancellationToken ct = default)
    {
        // 1. Student + branch scope (via enrollment)
        var studentExists = await db.Set<StudentEntity>().AsNoTracking()
            .AnyAsync(s => s.Id == studentId, ct);
        if (!studentExists) return Error.NotFound("Student", studentId.ToString());

        var enrollment = await db.Set<SchoolERP.Domain.Student.Entities.StudentEnrollment>().AsNoTracking()
            .Where(e => e.StudentId == studentId && e.AcademicYearId == academicYearId)
            .OrderByDescending(e => e.Id)
            .Select(e => new { e.ClassId, e.SectionId })
            .FirstOrDefaultAsync(ct);
        if (enrollment is null)
            return Error.Validation($"Student {studentId} has no enrollment for academic year {academicYearId}.");

        // 2. Resolve structure: override -> section -> class -> classgroup
        var structure = await ResolveStructureAsync(db, studentId, academicYearId,
            enrollment.ClassId, enrollment.SectionId, ct);
        if (structure is null)
            return Error.Validation($"No active fee structure found for student {studentId}.");

        // 3. Load fee heads (for name snapshot)
        var feeHeadIds = structure.Lines.Where(l => l.DeletedAt == null)
            .Select(l => l.FeeHeadId).Distinct().ToList();
        var feeHeads = await db.Set<FeeHeadEntity>().AsNoTracking()
            .Where(h => feeHeadIds.Contains(h.Id))
            .ToDictionaryAsync(h => h.Id, h => h, ct);

        // 4. Load approved concessions for student + AY
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

        // 5. Build source lines with frozen snapshots + per-line concession calc
        var lineDrafts = new List<InvoiceLineDraft>();
        var usageDrafts = new List<ConcessionUsageDraft>();
        decimal totalAmount = 0m;
        decimal totalConcession = 0m;

        foreach (var fl in structure.Lines.Where(l => l.DeletedAt == null))
        {
            if (!feeHeads.TryGetValue(fl.FeeHeadId, out var head)) continue;

            // Per-line concession: match applicable allocations by (FeeHeadId == line OR invoice-wide)
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
                LateFeeCapBase = netAmount,   // frozen at generation
            });

            totalAmount += fl.Amount;
            totalConcession += concessionAmount;

            // Record one usage per (allocation, line) with proportional split
            if (concessionAmount > 0 && applicableConcessions.Count > 0)
            {
                foreach (var alloc in allocations)
                {
                    if (!concessionsById.ContainsKey(alloc.ConcessionId)) continue;
                    var c = concessionsById[alloc.ConcessionId];
                    if (c.FeeHeadId.HasValue && c.FeeHeadId.Value != fl.FeeHeadId) continue;
                    // Split evenly across applicable allocations for this line
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
        // 1. Override
        var overrideId = await db.Set<SchoolERP.Domain.Fees.Entities.StudentFeeAssignment>().AsNoTracking()
            .Where(a => a.StudentId == studentId && a.AcademicYearId == academicYearId && a.IsActive)
            .Select(a => (long?)a.FeeStructureId)
            .FirstOrDefaultAsync(ct);

        if (overrideId.HasValue)
        {
            var s = await db.Set<FeeStructureEntity>().AsNoTracking().Include(x => x.Lines)
                .FirstOrDefaultAsync(x => x.Id == overrideId.Value && x.DeletedAt == null && x.Status == FeeStructureStatus.Active, ct);
            if (s is not null) return s;
        }

        // 2. Section -> Class -> ClassGroup
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
