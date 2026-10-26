using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace SchoolERP.Infrastructure.Persistence.Converters;

/// <summary>
/// Explicit enum <-> DB string converter.
/// Every enum stored in a column uses this so DB values are exact snake_case strings
/// (e.g. "one_time", "partially_paid", "stagnant_fixed"), never PascalCase.
/// </summary>
public sealed class EnumStringConverter<TEnum> : ValueConverter<TEnum, string>
    where TEnum : struct, Enum
{
    public EnumStringConverter(IReadOnlyDictionary<TEnum, string> map)
        : base(
            v => map[v],
            s => Reverse(map)[s])
    {
    }

    private static IReadOnlyDictionary<string, TEnum> Reverse(IReadOnlyDictionary<TEnum, string> map)
    {
        var reverse = new Dictionary<string, TEnum>(map.Count, StringComparer.OrdinalIgnoreCase);
        foreach (var kv in map) reverse[kv.Value] = kv.Key;
        return reverse;
    }
}
