namespace SchoolERP.Application.Common.Extensions;

public static class PatchHelperExtensions
{
    /// <summary>
    /// Agar newValue provided hai (null/empty nahi), toh setter action chalao.
    /// </summary>
    public static void PatchIfProvided(this string? newValue, Action<string> setter)
    {
        if (!string.IsNullOrWhiteSpace(newValue))
            setter(newValue.Trim());
    }
}