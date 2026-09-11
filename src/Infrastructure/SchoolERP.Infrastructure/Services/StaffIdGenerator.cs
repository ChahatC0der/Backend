using System.Text;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Staff.Entities;
using BranchEntity = SchoolERP.Domain.Tenants.Entities.Branch;

namespace SchoolERP.Infrastructure.Services;

public class StaffIdGenerator : IStaffIdGenerator
{
    /// <summary>
    /// Default format for Staff ID.
    /// Supports placeholders: {BRANCH_CODE}, {TENANT_CODE}, {YEAR}, {MONTH}, {SEQUENCE}
    /// Sequence supports padding: {SEQUENCE} → 5-digit default, or {SEQUENCE:3} → 3-digit
    /// </summary>
    public const string DefaultFormat = "{BRANCH_CODE}-STAFF-{YEAR}-{SEQUENCE}";
    public const int DefaultSequencePadding = 5;

    private readonly IApplicationDbContext _dbContext;

    public StaffIdGenerator(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<string> GenerateAsync(Guid branchId, CancellationToken cancellationToken = default)
    {
        // Future: read format from tenant settings
        // var format = await _settingsService.GetStaffIdFormatAsync(tenantId);
        return GenerateAsync(branchId, DefaultFormat, cancellationToken);
    }

    public async Task<string> GenerateAsync(Guid branchId, string format, CancellationToken cancellationToken = default)
    {
        // 1. Get branch code and tenant code
        var branch = await _dbContext.Set<BranchEntity>()
            .AsNoTracking()
            .IgnoreQueryFilters()  // allow cross-tenant lookup when needed
            .Where(b => b.Id == branchId && !b.IsDeleted)
            .Select(b => new { b.Code, b.TenantId })
            .FirstOrDefaultAsync(cancellationToken);

        if (branch == null)
            throw new InvalidOperationException($"Branch {branchId} not found.");

        var branchCode = branch.Code ?? "BR";
        var tenantCode = branch.TenantId.ToString().Substring(0, 6).ToUpperInvariant(); // fallback

        // 2. Determine year/month
        var now = DateTime.UtcNow;
        var year = now.Year;
        var month = now.Month;

        // 3. Compute next sequence for this branch + year
        var sequence = await GetNextSequenceAsync(branchId, year, cancellationToken);

        // 4. Apply format
        var result = ApplyFormat(format, new Dictionary<string, string>
        {
            ["BRANCH_CODE"] = branchCode,
            ["TENANT_CODE"] = tenantCode,
            ["YEAR"] = year.ToString(),
            ["MONTH"] = month.ToString("D2"),
            ["SEQUENCE"] = sequence.ToString() // placeholder; padding applied below
        }, sequence);

        // 5. Safety: ensure uniqueness (in case of race condition / manual conflict)
        if (await StaffIdExistsAsync(result, cancellationToken))
        {
            // Fallback: append a short unique suffix
            result = $"{result}-{Guid.NewGuid().ToString("N").Substring(0, 4).ToUpperInvariant()}";
        }

        return result;
    }

    // -----------------------------------------------------------
    // Private helpers
    // -----------------------------------------------------------

    private async Task<int> GetNextSequenceAsync(Guid branchId, int year, CancellationToken ct)
    {
        // Count staff created in this branch for this year
        // (Using CreatedAt year, fallback to JoiningDate.Year)
        var countThisYear = await _dbContext.Set<Staff>()
            .IgnoreQueryFilters()
            .Where(s => s.BranchId == branchId && s.CreatedAt.Year == year)
            .CountAsync(ct);

        return countThisYear + 1;
    }

    private static string ApplyFormat(string format, Dictionary<string, string> values, int sequence)
    {
        var result = format;

        foreach (var kvp in values)
        {
            // Skip SEQUENCE here, will handle padding separately
            if (kvp.Key == "SEQUENCE") continue;

            result = result.Replace($"{{{kvp.Key}}}", kvp.Value);
        }

        // Handle {SEQUENCE} and {SEQUENCE:n}
        // Matches {SEQUENCE} or {SEQUENCE:3}
        var sequencePattern = new System.Text.RegularExpressions.Regex(@"\{SEQUENCE(?::(\d+))?\}");
        result = sequencePattern.Replace(result, match =>
        {
            var padStr = match.Groups[1].Success ? match.Groups[1].Value : DefaultSequencePadding.ToString();
            var pad = int.TryParse(padStr, out var p) ? p : DefaultSequencePadding;
            return sequence.ToString($"D{pad}");
        });

        return result;
    }

    private async Task<bool> StaffIdExistsAsync(string staffId, CancellationToken ct)
    {
        return await _dbContext.Set<Staff>()
            .IgnoreQueryFilters()
            .AnyAsync(s => s.StaffId == staffId, ct);
    }
}