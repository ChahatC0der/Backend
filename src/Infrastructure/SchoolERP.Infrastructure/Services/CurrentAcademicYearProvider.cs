using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Infrastructure.Persistence;
using AcademicYearEntity = SchoolERP.Domain.Academic.Entities.AcademicYear;

namespace SchoolERP.Infrastructure.Services;

public sealed class CurrentAcademicYearProvider : ICurrentAcademicYearProvider
{
    private readonly AppDbContext _db;
    private readonly ICurrentBranchService _branch;

    public CurrentAcademicYearProvider(AppDbContext db, ICurrentBranchService branch)
    {
        _db = db; _branch = branch;
    }

    public async Task<CurrentAcademicYearInfo?> GetCurrentAsync(CancellationToken ct = default)
    {
        var branchId = _branch.GetBranchId();
        if (branchId is null || branchId == Guid.Empty) return null;

        var ay = await _db.Set<AcademicYearEntity>().AsNoTracking()
            .Where(x => x.BranchId == branchId.Value && x.IsCurrent)
            .OrderByDescending(x => x.StartDate)
            .Select(x => new { x.Id, x.Name, x.StartDate, x.EndDate })
            .FirstOrDefaultAsync(ct);

        return ay is null ? null : ToInfo(ay.Id, ay.Name, ay.StartDate, ay.EndDate);
    }

    public async Task<CurrentAcademicYearInfo?> GetByIdAsync(long academicYearId, CancellationToken ct = default)
    {
        var ay = await _db.Set<AcademicYearEntity>().AsNoTracking()
            .Where(x => x.Id == academicYearId)
            .Select(x => new { x.Id, x.Name, x.StartDate, x.EndDate })
            .FirstOrDefaultAsync(ct);

        return ay is null ? null : ToInfo(ay.Id, ay.Name, ay.StartDate, ay.EndDate);
    }

    private static CurrentAcademicYearInfo ToInfo(long id, string name, DateOnly start, DateOnly end)
    {
        var startShort = start.Year % 100;
        var endShort = end.Year % 100;
        return new CurrentAcademicYearInfo(
            id,
            $"{startShort:D2}{endShort:D2}",
            name,
            start,
            end);
    }
}
