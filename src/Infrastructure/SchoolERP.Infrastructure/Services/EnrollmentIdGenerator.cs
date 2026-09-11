using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Student.Entities;
using BranchEntity = SchoolERP.Domain.Tenants.Entities.Branch;

namespace SchoolERP.Infrastructure.Services;

public class EnrollmentIdGenerator : IEnrollmentIdGenerator
{
    public const string DefaultFormat = "{BRANCH_CODE}-{YEAR}-{SEQUENCE}";
    public const int DefaultSequencePadding = 5;

    private readonly IApplicationDbContext _dbContext;

    public EnrollmentIdGenerator(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<string> GenerateAsync(Guid branchId, CancellationToken cancellationToken = default)
        => GenerateAsync(branchId, DefaultFormat, cancellationToken);

    public async Task<string> GenerateAsync(Guid branchId, string format, CancellationToken cancellationToken = default)
    {
        var branch = await _dbContext.Set<BranchEntity>()
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(b => b.Id == branchId && !b.IsDeleted)
            .Select(b => new { b.Code })
            .FirstOrDefaultAsync(cancellationToken);

        if (branch == null)
            throw new InvalidOperationException($"Branch {branchId} not found.");

        var branchCode = branch.Code ?? "BR";
        var year = DateTime.UtcNow.Year;

        var countThisYear = await _dbContext.Set<Student>()
            .IgnoreQueryFilters()
            .Where(s => s.BranchId == branchId && s.CreatedAt.Year == year)
            .CountAsync(cancellationToken);

        var sequence = countThisYear + 1;

        var result = format
            .Replace("{BRANCH_CODE}", branchCode)
            .Replace("{YEAR}", year.ToString());

        var sequencePattern = new Regex(@"\{SEQUENCE(?::(\d+))?\}");
        result = sequencePattern.Replace(result, match =>
        {
            var padStr = match.Groups[1].Success ? match.Groups[1].Value : DefaultSequencePadding.ToString();
            var pad = int.TryParse(padStr, out var p) ? p : DefaultSequencePadding;
            return sequence.ToString($"D{pad}");
        });

        if (await _dbContext.Set<Student>().IgnoreQueryFilters().AnyAsync(s => s.EnrollmentId == result, cancellationToken))
        {
            result = $"{result}-{Guid.NewGuid().ToString("N").Substring(0, 4).ToUpperInvariant()}";
        }

        return result;
    }
}
