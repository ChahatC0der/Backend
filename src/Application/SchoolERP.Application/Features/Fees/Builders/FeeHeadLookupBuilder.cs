using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FeeHeadEntity = SchoolERP.Domain.Fees.Entities.FeeHead;

namespace SchoolERP.Application.Features.Fees.Builders;

/// <summary>
/// Builds the FeeHead lookup used by FeeStructure response mappings.
/// Keeps the query branch-scoped and loads each required FeeHead in one database round-trip.
/// Inactive FeeHeads are included for historical FeeStructureLines; soft-deleted FeeHeads are excluded.
/// </summary>
public static class FeeHeadLookupBuilder
{
    public static async Task<IReadOnlyDictionary<long, (string Name, string Code)>> BuildAsync(
        IApplicationDbContext db,
        Guid branchId,
        IEnumerable<long> feeHeadIds,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(feeHeadIds);

        if (branchId == Guid.Empty)
            throw new ArgumentException("A valid branch ID is required.", nameof(branchId));

        var ids = feeHeadIds.Distinct().ToArray();
        if (ids.Length == 0)
            return new Dictionary<long, (string Name, string Code)>();

        var rows = await db.Set<FeeHeadEntity>()
            .AsNoTracking()
            .Where(h =>
                h.BranchId == branchId &&
                h.DeletedAt == null &&
                ids.Contains(h.Id))
            .Select(h => new
            {
                h.Id,
                h.Name,
                h.Code
            })
            .ToListAsync(cancellationToken);

        return rows.ToDictionary(
            h => h.Id,
            h => (Name: h.Name, Code: h.Code));
    }
}
