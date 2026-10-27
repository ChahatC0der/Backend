using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Shared.Results;
using ParentEntity = SchoolERP.Domain.Student.Entities.Parent;

namespace SchoolERP.Application.Features.Fees.Builders;

/// <summary>
/// Verifies the logged-in user is a parent/guardian of the requested student.
/// Phase 1 mapping assumption: Parent entity has (UserId, StudentId)  one row per link.
/// If your Parent entity uses a different shape (Children navigation, join table, etc.),
/// ONLY this file needs to change.
/// </summary>
public static class ParentChildResolver
{
    /// <summary>Returns true if currentUserId is a parent of studentId; otherwise a Result error.</summary>
    public static async Task<Result> EnsureParentAsync(
        IApplicationDbContext db,
        long currentUserId,
        long studentId,
        CancellationToken ct = default)
    {
        if (currentUserId <= 0) return Result.Failure(Error.Unauthorized("Current user is not resolved."));

        var isParent = await db.Set<ParentEntity>().AsNoTracking()
            .AnyAsync(p => p.UserId == currentUserId && p.StudentId == studentId, ct);

        if (!isParent)
            return Result.Failure(Error.Unauthorized("You do not have access to this student."));

        return Result.Success(true);
    }
}
