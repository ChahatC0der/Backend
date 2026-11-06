
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Fees.Enums;

using ClassEntity = SchoolERP.Domain.Academic.Entities.Class;
using ClassGroupEntity = SchoolERP.Domain.Academic.Entities.ClassGroup;
using SectionEntity = SchoolERP.Domain.Academic.Entities.Section;
using FeeStructureEntity = SchoolERP.Domain.Fees.Entities.FeeStructure;

namespace SchoolERP.Application.Features.Fees.Builders;

/// <summary>
/// Resolves the most specific active FeeStructure applicable
/// to a student's academic enrollment and requested billing period.
///
/// Resolution priority:
/// 1. Section
/// 2. Class
/// 3. ClassGroup
///
/// This component is responsible only for data resolution.
/// It does not calculate fees, concessions, proration,
/// invoice totals, or persist invoices.
/// </summary>
public static class FeeStructureResolver
{
    /// <summary>
    /// Finds the most specific applicable FeeStructure.
    ///
    /// Returns null when:
    /// - The class does not exist in the requested branch.
    /// - The supplied section does not belong to that class.
    /// - No applicable active structure is found.
    ///
    /// Student-specific FeeStructure overrides are intentionally
    /// excluded because that feature is deferred from the current scope.
    /// </summary>
    public static async Task<FeeStructureEntity?> ResolveAsync(
        IApplicationDbContext db,
        Guid branchId,
        long academicYearId,
        long classId,
        long? sectionId,
        BillingPeriod billingPeriod,
        CancellationToken ct = default)
    {
        // Block 1: Validate the input.
        // A valid billing period is required to check FeeStructure dates.
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(billingPeriod);

        if (branchId == Guid.Empty)
        {
            throw new ArgumentException(
                "BranchId is required.",
                nameof(branchId));
        }

        if (academicYearId <= 0 || classId <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(academicYearId),
                "AcademicYearId and ClassId must be positive.");
        }

        if (billingPeriod.EndDate < billingPeriod.StartDate)
        {
            throw new ArgumentException(
                "Billing period end date cannot be earlier than start date.",
                nameof(billingPeriod));
        }

        // Block 2: Convert the billing period boundaries into
        // DateTime values suitable for comparison with FeeStructure.
        //
        // Using the exclusive next-day boundary ensures that
        // structures applicable at any time on the last day
        // are considered.
        var periodStart = billingPeriod.StartDate.ToDateTime(
            TimeOnly.MinValue);

        var periodEndExclusive = billingPeriod.EndDate
            .AddDays(1)
            .ToDateTime(TimeOnly.MinValue);

        // Block 3: Load the student's class information.
        //
        // ClassGroupId is NOT stored directly on StudentEnrollment.
        // It must be resolved through the enrolled Class.
        //
        // Branch and soft-delete checks ensure we do not resolve
        // a class belonging to another branch.
        var classInfo = await db.Set<ClassEntity>()
            .AsNoTracking()
            .Where(c =>
                c.Id == classId &&
                c.BranchId == branchId &&
                c.DeletedAt == null)
            .Select(c => new
            {
                c.Id,
                c.ClassGroupId
            })
            .FirstOrDefaultAsync(ct);

        // Without a valid class, no FeeStructure can be resolved.
        if (classInfo is null)
        {
            return null;
        }

        // Block 4: Validate the section when one is supplied.
        //
        // A section must belong to the requested class
        // and the same branch.
        if (sectionId.HasValue)
        {
            var validSection = await db.Set<SectionEntity>()
                .AsNoTracking()
                .AnyAsync(s =>
                    s.Id == sectionId.Value &&
                    s.ClassId == classInfo.Id &&
                    s.BranchId == branchId &&
                    s.DeletedAt == null,
                    ct);

            if (!validSection)
            {
                return null;
            }
        }

        // Block 5: Validate the ClassGroup reference.
        //
        // A class may have no ClassGroup.
        // An inactive, deleted, or cross-branch ClassGroup
        // must not be used as a fallback.
        long? validClassGroupId = null;

        if (classInfo.ClassGroupId.HasValue)
        {
            var groupIsValid = await db.Set<ClassGroupEntity>()
                .AsNoTracking()
                .AnyAsync(g =>
                    g.Id == classInfo.ClassGroupId.Value &&
                    g.BranchId == branchId &&
                    g.IsActive &&
                    g.DeletedAt == null,
                    ct);

            if (groupIsValid)
            {
                validClassGroupId = classInfo.ClassGroupId;
            }
        }

        // Block 6: Build the common FeeStructure query.
        //
        // Every candidate must:
        // - Belong to the current branch.
        // - Belong to the requested Academic Year.
        // - Be active and not soft-deleted.
        // - Overlap the requested billing period.
        //
        // FeeStructure lines are loaded with the parent entity.
        // Deleted lines will be excluded by the invoice builder.
        var candidates = db.Set<FeeStructureEntity>()
            .AsNoTracking()
            .Include(x => x.Lines)
            .Where(x =>
                x.BranchId == branchId &&
                x.AcademicYearId == academicYearId &&
                x.Status == FeeStructureStatus.Active &&
                x.DeletedAt == null &&
                x.ApplicableFrom < periodEndExclusive &&
                x.ApplicableTo >= periodStart);

        // Block 7: Try to resolve the Section-level structure.
        //
        // A section-level structure must match both ClassId
        // and SectionId.
        //
        // When both group-specific and group-neutral records exist,
        // prefer the one explicitly matching the student's group.
        if (sectionId.HasValue)
        {
            var sectionStructure = await candidates
                .Where(x =>
                    x.ClassId == classInfo.Id &&
                    x.SectionId == sectionId.Value &&
                    (x.ClassGroupId == null ||
                     x.ClassGroupId == validClassGroupId))
                .OrderByDescending(x =>
                    x.ClassGroupId == validClassGroupId)
                .FirstOrDefaultAsync(ct);

            if (sectionStructure is not null)
            {
                return sectionStructure;
            }
        }

        // Block 8: Try to resolve the Class-level structure.
        //
        // The structure must match the student's ClassId.
        // SectionId must be null because this is a class-level
        // configuration, not a section-specific configuration.
        var classStructure = await candidates
            .Where(x =>
                x.ClassId == classInfo.Id &&
                x.SectionId == null &&
                (x.ClassGroupId == null ||
                 x.ClassGroupId == validClassGroupId))
            .OrderByDescending(x =>
                x.ClassGroupId == validClassGroupId)
            .FirstOrDefaultAsync(ct);

        if (classStructure is not null)
        {
            return classStructure;
        }

        // Block 9: Try to resolve the ClassGroup-level structure.
        //
        // This is the final fallback.
        // The ClassGroupId must exactly match the group resolved
        // from the student's actual Class.
        if (validClassGroupId.HasValue)
        {
            var groupStructure = await candidates
                .Where(x =>
                    x.ClassGroupId == validClassGroupId.Value &&
                    x.ClassId == null &&
                    x.SectionId == null)
                .FirstOrDefaultAsync(ct);

            if (groupStructure is not null)
            {
                return groupStructure;
            }
        }

        // Block 10: No applicable structure was found.
        //
        // The calling invoice-generation service/handler
        // will convert this into an appropriate validation error.
        return null;
    }
}
