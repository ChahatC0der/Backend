namespace SchoolERP.Application.Common.Interfaces;

public sealed record CurrentAcademicYearInfo(
    long Id,
    string Code,        // e.g. "2526"
    string Name,        // e.g. "2025-26"
    DateOnly StartDate,
    DateOnly EndDate);

public interface ICurrentAcademicYearProvider
{
    Task<CurrentAcademicYearInfo?> GetCurrentAsync(CancellationToken ct = default);
    Task<CurrentAcademicYearInfo?> GetByIdAsync(long academicYearId, CancellationToken ct = default);
}
