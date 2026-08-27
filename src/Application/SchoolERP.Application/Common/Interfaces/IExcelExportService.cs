namespace SchoolERP.Application.Common.Interfaces;

public interface IExcelExportService
{
    byte[] Export<T>(IEnumerable<T> items, string sheetName);
}