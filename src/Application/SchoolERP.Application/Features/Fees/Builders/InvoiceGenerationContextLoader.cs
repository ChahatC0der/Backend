
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Fees.Enums;
using SchoolERP.Domain.Shared.Results;

using StudentEntity = SchoolERP.Domain.Student.Entities.Student;
using StudentEnrollmentEntity = SchoolERP.Domain.Student.Entities.StudentEnrollment;
using FeeHeadEntity = SchoolERP.Domain.Fees.Entities.FeeHead;
using ConcessionEntity = SchoolERP.Domain.Fees.Entities.Concession;
using ConcessionAllocationEntity = SchoolERP.Domain.Fees.Entities.ConcessionAllocation;

namespace SchoolERP.Application.Features.Fees.Builders;

/// <summary>
/// Loads all database data required for invoice generation
/// and assembles an InvoiceGenerationContext.
///
/// Responsibilities:
/// - Load the student.
/// - Find enrollment records overlapping the billing period.
/// - Resolve the applicable FeeStructure.
/// - Load referenced FeeHeads.
/// - Load approved concession allocations and applicable concessions.
///
/// Business calculations are delegated to separate policies and builders.
/// This component does not create or persist invoices.
/// </summary>
public static class InvoiceGenerationContextLoader
{
    /// <summary>
    /// Loads the complete invoice generation context for one student
    /// and one billing period.
    ///
    /// Returns a validation or not-found error when the required
    /// student, enrollment, structure, or FeeHead data is unavailable.
    /// </summary>

    public static async Task<Result<InvoiceGenerationContext>> LoadAsync(
        IApplicationDbContext db,
        Guid branchId,
        long studentId,
        long academicYearId,
        BillingPeriod billingPeriod,
        DateTime concessionEvaluationDate,

        // Optional additional FeeHead code, such as TRANSPORT.
        // This allows the loader to retrieve a FeeHead that is not
        // part of the regular FeeStructure.
        string? additionalFeeHeadCode = null,

        CancellationToken ct = default)

    {
        // ---------------------------------------------------------
        // BLOCK 1: Validate input arguments.
        // Invalid identifiers or date ranges must not reach queries.
        // ---------------------------------------------------------

        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(billingPeriod);

        if (branchId == Guid.Empty)
        {
            return Error.Validation("BranchId is required.");
        }

        if (studentId <= 0 || academicYearId <= 0)
        {
            return Error.Validation(
                "StudentId and AcademicYearId must be positive.");
        }

        if (billingPeriod.EndDate < billingPeriod.StartDate)
        {
            return Error.Validation(
                "Billing period end date cannot be earlier than start date.");
        }

        // ---------------------------------------------------------
        // BLOCK 2: Load the student.
        //
        // The Student entity is branch and tenant scoped.
        // AppDbContext global query filters enforce both scopes.
        // Explicit BranchId and DeletedAt checks are retained
        // for clarity and defensive filtering.
        // ---------------------------------------------------------

        var student = await db.Set<StudentEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(s =>
                s.Id == studentId &&
                s.BranchId == branchId &&
                s.DeletedAt == null,
                ct);

        if (student is null)
        {
            return Error.NotFound(
                "Student",
                studentId.ToString());
        }

        // ---------------------------------------------------------
        // BLOCK 3: Load enrollment records overlapping the period.
        //
        // Do not filter only by IsCurrent.
        // Historical enrollment records may be needed when
        // generating invoices for previous billing periods.
        //
        // Enrollment must overlap at least one day of the
        // requested billing period.
        // ---------------------------------------------------------

        var enrollments = await db.Set<StudentEnrollmentEntity>()
            .AsNoTracking()
            .Where(e =>
                e.StudentId == studentId &&
                e.AcademicYearId == academicYearId &&
                e.BranchId == branchId &&
                e.DeletedAt == null &&
                e.EnrolledAt <= billingPeriod.EndDate &&
                (e.LeftAt == null ||
                 e.LeftAt >= billingPeriod.StartDate))
            .OrderBy(e => e.EnrolledAt)
            .ThenBy(e => e.Id)
            .ToListAsync(ct);

        if (enrollments.Count == 0)
        {
            return Error.Validation(
                $"Student {studentId} has no enrollment overlapping " +
                $"billing period {billingPeriod.StartDate:yyyy-MM-dd} " +
                $"to {billingPeriod.EndDate:yyyy-MM-dd}.");
        }

        // ---------------------------------------------------------
        // BLOCK 4: Resolve the enrollment and its FeeStructure.
        //
        // Different enrollment records may point to different
        // classes or sections.
        //
        // Resolve each candidate independently. Student eligibility
        // and FeeStructure applicability are checked separately.
        //
        // We intentionally reject multiple eligible enrollments
        // instead of silently selecting an arbitrary class/section.
        // ---------------------------------------------------------

        var eligibleCandidates = new List<(
            StudentEnrollmentEntity Enrollment,
            SchoolERP.Domain.Fees.Entities.FeeStructure Structure)>();

        foreach (var enrollment in enrollments)
        {
            // Resolve the most specific structure:
            // Section -> Class -> ClassGroup.
            var structure = await FeeStructureResolver.ResolveAsync(
                db,
                branchId,
                academicYearId,
                enrollment.ClassId,
                enrollment.SectionId,
                billingPeriod,
                ct);

            if (structure is null)
            {
                continue;
            }

            // Apply the separate student eligibility policy.
            // No proration is calculated at this stage.
            var eligible = StudentInvoiceEligibilityPolicy.IsEligible(
                enrollment.EnrolledAt,
                enrollment.LeftAt,
                structure.ApplicableFrom,
                structure.ApplicableTo,
                billingPeriod);

            if (!eligible)
            {
                continue;
            }

            eligibleCandidates.Add((enrollment, structure));
        }

        // No enrollment has both an applicable structure
        // and valid eligibility for this billing period.
        if (eligibleCandidates.Count == 0)
        {
            return Error.Validation(
                $"No applicable active FeeStructure found for student " +
                $"{studentId} during billing period " +
                $"{billingPeriod.StartDate:yyyy-MM-dd} to " +
                $"{billingPeriod.EndDate:yyyy-MM-dd}.");
        }

        // More than one matching enrollment indicates ambiguous
        // enrollment data, such as overlapping class transfers.
        // Do not generate a potentially incorrect invoice.
        if (eligibleCandidates.Count > 1)
        {
            return Error.Validation(
                $"Multiple eligible enrollment records found for student " +
                $"{studentId} during the requested billing period. " +
                "Resolve overlapping enrollments before generating the invoice.");
        }

        // Exactly one enrollment and structure must be applicable.
        var selectedCandidate = eligibleCandidates[0];

        var selectedEnrollment = selectedCandidate.Enrollment;
        var selectedStructure = selectedCandidate.Structure;

        // ---------------------------------------------------------
        // BLOCK 5: Identify active structure lines.
        //
        // Deleted FeeStructureLines must not contribute to invoices.
        // The actual fee calculations will happen in the draft builder.
        // ---------------------------------------------------------

        var activeStructureLines = selectedStructure.Lines
            .Where(line => line.DeletedAt == null)
            .ToList();

        if (activeStructureLines.Count == 0)
        {
            return Error.Validation(
                $"FeeStructure {selectedStructure.Id} has no active lines.");
        }

        // Collect distinct FeeHead IDs to avoid loading
        // the same FeeHead multiple times.
        var feeHeadIds = activeStructureLines
            .Select(line => line.FeeHeadId)
            .Distinct()
            .ToList();


        // BLOCK 6: Load all required FeeHeads.
        //
        // Load both:
        // 1. FeeHeads referenced by active FeeStructureLines.
        // 2. An optional additional FeeHead required by another
        //    invoice component, such as the TransportFeeLineBuilder.
        //
        // Branch and soft-delete filters prevent cross-branch
        // or deleted FeeHeads from entering the invoice context.

        var feeHeads = await db.Set<FeeHeadEntity>()
            .AsNoTracking()
            .Where(h =>
                h.BranchId == branchId &&
                h.DeletedAt == null &&
                (
                    feeHeadIds.Contains(h.Id) ||
                    (
                        !string.IsNullOrWhiteSpace(additionalFeeHeadCode) &&
                        h.Code == additionalFeeHeadCode
                    )
                ))
            .ToDictionaryAsync(
                h => h.Id,
                h => h,
                ct);

        // Every active FeeStructureLine must resolve to a FeeHead.
        // Do not silently skip a missing or inaccessible FeeHead.
        var missingFeeHeadIds = feeHeadIds
            .Where(id => !feeHeads.ContainsKey(id))
            .ToList();

        if (missingFeeHeadIds.Count > 0)
        {
            return Error.Validation(
                $"FeeStructure {selectedStructure.Id} references " +
                $"missing, deleted, or inaccessible FeeHeads: " +
                $"{string.Join(", ", missingFeeHeadIds)}.");
        }


        // ---------------------------------------------------------
        // BLOCK 7: Load approved concession allocations.
        //
        // ConcessionAllocation is associated with the student
        // and Academic Year.
        //
        // This entity does not implement IMustHaveBranch,
        // so the student and Academic Year identifiers are used
        // to scope this lookup.
        // ---------------------------------------------------------

        var concessionAllocations =
            await db.Set<ConcessionAllocationEntity>()
                .AsNoTracking()
                .Where(a =>
                    a.StudentId == studentId &&
                    a.AcademicYearId == academicYearId &&
                    a.Status == ConcessionAllocationStatus.Approved)
                .ToListAsync(ct);

        // ---------------------------------------------------------
        // BLOCK 8: Load currently applicable concession definitions.
        //
        // A concession must:
        // - Be linked to an approved allocation.
        // - Not be soft-deleted.
        // - Have started by the evaluation date, if StartDate exists.
        // - Not have expired before the evaluation date.
        //
        // Concession is branch-scoped. AppDbContext global filters
        // enforce branch isolation.
        // ---------------------------------------------------------

        var concessionIds = concessionAllocations
            .Select(a => a.ConcessionId)
            .Distinct()
            .ToList();

        var concessions = concessionIds.Count == 0
            ? new Dictionary<long, ConcessionEntity>()
            : await db.Set<ConcessionEntity>()
                .AsNoTracking()
                .Where(c =>
                    concessionIds.Contains(c.Id) &&
                    c.DeletedAt == null &&
                    (c.StartDate == null ||
                     c.StartDate <= concessionEvaluationDate) &&
                    (c.EndDate == null ||
                     c.EndDate >= concessionEvaluationDate))
                .ToDictionaryAsync(
                    c => c.Id,
                    c => c,
                    ct);

        // ---------------------------------------------------------
        // BLOCK 9: Assemble the complete in-memory context.
        //
        // The InvoiceDraftBuilder will consume this context.
        // No invoice entities are created or saved here.
        // ---------------------------------------------------------

        var context = new InvoiceGenerationContext(
            Student: student,
            Enrollment: selectedEnrollment,
            FeeStructure: selectedStructure,
            FeeHeads: feeHeads,
            ConcessionAllocations: concessionAllocations.AsReadOnly(),
            Concessions: concessions);

        return Result.Success(context);
    }
}
