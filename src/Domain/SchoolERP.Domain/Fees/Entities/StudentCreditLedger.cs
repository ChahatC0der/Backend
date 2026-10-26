using SchoolERP.Domain.Common;
using SchoolERP.Domain.Fees.Enums;

namespace SchoolERP.Domain.Fees.Entities;

/// <summary>
/// Append-only tenant-scoped wallet ledger. No BranchId, no soft delete, no UpdatedAt.
/// Physical dbo.StudentCreditLedger.
/// BalanceAfter is denormalized for O(1) reads — latest row (by Id DESC) is current balance.
/// AcademicYearId is nullable — balances carry forward across academic years.
/// </summary>
public sealed class StudentCreditLedger : IdEntity
{
    public Guid TenantId { get; set; }
    public long StudentId { get; set; }
    public long? AcademicYearId { get; set; }
    public decimal Amount { get; set; }
    public decimal BalanceAfter { get; set; }
    public CreditTransactionType TransactionType { get; set; }
    public long? ReferenceInvoiceId { get; set; }
    public long? ReferencePaymentId { get; set; }
    public string? Reason { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
