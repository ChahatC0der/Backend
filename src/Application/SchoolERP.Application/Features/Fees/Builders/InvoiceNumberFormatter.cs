namespace SchoolERP.Application.Features.Fees.Builders;

/// <summary>
/// Formats sequence numbers into invoice/receipt numbers.
/// Uniqueness scope: per (TenantId, BranchId), enforced by DB unique indexes.
/// </summary>
public static class InvoiceNumberFormatter
{
    public static string FormatInvoice(string branchCode, string ayShort, long sequence)
        => $"{branchCode}-{ayShort}-{sequence:D6}";

    public static string FormatReceipt(string branchCode, string ayShort, long sequence)
        => $"{branchCode}-RCP-{ayShort}-{sequence:D6}";
}
