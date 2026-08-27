using System.Text;
using System.Reflection;

namespace SchoolERP.Application.Common.Extensions;

public static class CsvExportHelper
{
    /// <summary>
    /// Kisi bhi list of objects ko CSV bytes mein convert karta hai — 
    /// public properties automatically columns ban jate hain.
    /// </summary>
    public static byte[] ToCsv<T>(this IEnumerable<T> items)
    {
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var csv = new StringBuilder();

        // Header row
        csv.AppendLine(string.Join(",", properties.Select(p => EscapeCsvField(p.Name))));

        // Data rows
        foreach (var item in items)
        {
            var values = properties.Select(p =>
            {
                var value = p.GetValue(item);
                var stringValue = value switch
                {
                    null => string.Empty,
                    DateTime dt => dt.ToString("yyyy-MM-dd HH:mm:ss"),
                    _ => value.ToString()
                };
                return EscapeCsvField(stringValue);
            });

            csv.AppendLine(string.Join(",", values));
        }

        return Encoding.UTF8.GetBytes(csv.ToString());
    }

    private static string EscapeCsvField(string? field)
    {
        if (string.IsNullOrEmpty(field))
            return string.Empty;

        if (field.Contains(',') || field.Contains('"') || field.Contains('\n') || field.Contains('\r'))
            return $"\"{field.Replace("\"", "\"\"")}\"";

        return field;
    }
}