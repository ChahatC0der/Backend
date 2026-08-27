using ClosedXML.Excel;
using System.Reflection;
using SchoolERP.Application.Common.Interfaces;

namespace SchoolERP.Infrastructure.Services;

public class ClosedXmlExportService : IExcelExportService
{
    public byte[] Export<T>(IEnumerable<T> items, string sheetName)
    {
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add(sheetName);

        for (int col = 0; col < properties.Length; col++)
        {
            var cell = worksheet.Cell(1, col + 1);
            cell.Value = properties[col].Name;
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightGray;
        }

        var itemsList = items.ToList();
        for (int row = 0; row < itemsList.Count; row++)
        {
            for (int col = 0; col < properties.Length; col++)
            {
                var value = properties[col].GetValue(itemsList[row]);
                var cell = worksheet.Cell(row + 2, col + 1);

                if (value is DateTime dt)
                    cell.Value = dt;
                else
                    cell.Value = value?.ToString() ?? string.Empty;
            }
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}