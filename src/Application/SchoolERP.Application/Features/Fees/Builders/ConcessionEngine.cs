using SchoolERP.Domain.Fees.Enums;
using SchoolERP.Domain.Fees.ValueObjects;
using ConcessionEntity = SchoolERP.Domain.Fees.Entities.Concession;

namespace SchoolERP.Application.Features.Fees.Builders;

public sealed record ConcessionLineInput(long LineId, long FeeHeadId, decimal GrossAmount);

public sealed record ConcessionLineResult(
    long LineId, decimal GrossAmount, decimal ConcessionAmount, decimal NetAmount);

/// Lines[i] ↔ input[i] — order preserved.
public sealed record ConcessionResult(
    IReadOnlyList<ConcessionLineResult> Lines,
    decimal TotalGross, decimal TotalConcession, decimal TotalNet);

/// <summary>
/// Invoice-level concession math. PURE — no I/O, no clock.
/// WHO (scope + dedup) builder karta hai; ye sirf KITNA nikaalti hai.
/// </summary>
public static class ConcessionEngine
{
    public static ConcessionResult Evaluate(
        IReadOnlyList<ConcessionLineInput> lines,
        IReadOnlyList<ConcessionEntity> concessions)
    {
        if (lines is null || lines.Count == 0)
            return new ConcessionResult(Array.Empty<ConcessionLineResult>(), 0m, 0m, 0m);

        if (concessions is null || concessions.Count == 0)
            return Zero(lines);

        // PHASE 1 — PARTITION
        // (a) per-line: head-specific (koi bhi type) + invoice-level PERCENTAGE (linear, exact split)
        var perLine = concessions
            .Where(c => c.FeeHeadId is not null
                     || c.CalculationType == ConcessionCalculationType.Percentage)
            .DistinctBy(c => c.Id)      // double-approval safety net
            .ToList();

        // (b) pool: SIRF invoice-level FIXED — yehi bug tha (₹200 × N lines)
        var poolItems = concessions
            .Where(c => c.FeeHeadId is null
                     && c.CalculationType == ConcessionCalculationType.Fixed)
            .DistinctBy(c => c.Id)
            .ToList();

        // PHASE 2 — PER-LINE (atom calculator ko delegate)
        var pass1 = new List<(ConcessionLineInput Line, decimal Head, decimal Remaining)>(lines.Count);
        foreach (var line in lines)
        {
            var applicable = perLine
                .Where(c => c.FeeHeadId == line.FeeHeadId || c.FeeHeadId is null)
                .ToList();

            decimal head = ConcessionCalculator.Calculate(line.GrossAmount, applicable);
            pass1.Add((line, head, line.GrossAmount - head));
        }

        // PHASE 3 — POOL: invoice-level Fixed EK baar, total par
        decimal pool = Money.Round(poolItems.Sum(c => c.Value));
        decimal remainingTotal = pass1.Sum(x => x.Remaining);
        decimal cappedPool = Money.Min(pool, remainingTotal);   // over-discount lock
        var shares = DistributePool(pass1, cappedPool);

        // PHASE 4 — COMBINE
        var results = new List<ConcessionLineResult>(lines.Count);
        decimal totalGross = 0m, totalConcession = 0m;

        for (int i = 0; i < pass1.Count; i++)
        {
            var (line, head, _) = pass1[i];
            decimal concession = Money.CapAt(head + shares[i], line.GrossAmount);
            results.Add(new ConcessionLineResult(line.LineId, line.GrossAmount,
                concession, line.GrossAmount - concession));
            totalGross += line.GrossAmount;
            totalConcession += concession;
        }

        totalGross = Money.Round(totalGross);
        totalConcession = Money.Round(totalConcession);
        return new ConcessionResult(results, totalGross, totalConcession,
            Money.Round(totalGross - totalConcession));
    }

    /// share ∝ head-discount ke BAAD bacha amount; last line paisa settle karti hai.
    /// Σ(shares) == pool exact. Koi net negative nahi.
    private static decimal[] DistributePool(
        List<(ConcessionLineInput Line, decimal Head, decimal Remaining)> pass1,
        decimal pool)
    {
        var shares = new decimal[pass1.Count];
        decimal remainingTotal = pass1.Sum(x => x.Remaining);
        if (pool <= 0m || remainingTotal <= 0m) return shares;

        decimal allocated = 0m;
        for (int i = 0; i < shares.Length; i++)
        {
            decimal remaining = pass1[i].Remaining;
            if (remaining <= 0m) continue;                              // fully-discounted line

            if (i == shares.Length - 1)
                shares[i] = Money.CapAt(pool - allocated, remaining);   // rounding yahan settle
            else
            {
                shares[i] = Money.Min(Money.Round(pool * remaining / remainingTotal), remaining);
                allocated += shares[i];
            }
        }
        return shares;
    }

    private static ConcessionResult Zero(IReadOnlyList<ConcessionLineInput> lines)
    {
        var results = lines
            .Select(l => new ConcessionLineResult(l.LineId, l.GrossAmount, 0m, l.GrossAmount))
            .ToList();
        decimal gross = Money.Round(lines.Sum(l => l.GrossAmount));
        return new ConcessionResult(results, gross, 0m, gross);
    }
}