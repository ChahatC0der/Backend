using SchoolERP.Application.Common.Interfaces;

namespace SchoolERP.Infrastructure.Services;

/// <summary>
/// STUB — Phase 0. Full implementation lands in Phase 2 (reads AcademicYears.IsCurrent).
/// Registered now so DI resolves during Phase 1.
/// </summary>
public sealed class CurrentAcademicYearProvider : ICurrentAcademicYearProvider
{
    public Task<CurrentAcademicYearInfo?> GetCurrentAsync(CancellationToken ct = default)
        => throw new NotImplementedException("CurrentAcademicYearProvider: implemented in Phase 2.");

    public Task<CurrentAcademicYearInfo?> GetByIdAsync(long academicYearId, CancellationToken ct = default)
        => throw new NotImplementedException("CurrentAcademicYearProvider: implemented in Phase 2.");
}
