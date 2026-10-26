namespace SchoolERP.Domain.Fees.ValueObjects;

/// <summary>
/// Single rounding point for all monetary math.
/// Always 2 decimal places, MidpointRounding.AwayFromZero.
/// Do not scatter Math.Round calls across handlers.
/// </summary>
public static class Money
{
    public static decimal Round(decimal value)
        => Math.Round(value, 2, MidpointRounding.AwayFromZero);

    public static decimal Min(decimal a, decimal b) => a < b ? a : b;

    public static decimal CapAt(decimal value, decimal cap)
        => value < 0 ? 0 : (value > cap ? cap : value);
}
