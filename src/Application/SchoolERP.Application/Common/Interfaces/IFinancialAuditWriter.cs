using SchoolERP.Domain.Fees.Enums;

namespace SchoolERP.Application.Common.Interfaces;

/// <summary>
/// AUDIT CONTRACT ONLY — Phase 0 deliverable.
///
/// IMPORTANT:
///   - No Infrastructure implementation exists yet (deferred until audit phase).
///   - Do NOT uncomment _auditWriter.LogAsync(...) calls inside handlers yet.
///   - Handler code should carry them as:   // TODO (audit): await _auditWriter.LogAsync(...);
///   - Once the audit phase begins, add FinancialAuditWriter implementation,
///     register it in Infrastructure DI, then uncomment handler calls.
/// </summary>
public interface IFinancialAuditWriter
{
    Task LogAsync(
        string tableName,
        long recordId,
        FinancialAuditAction action,
        object? oldValues,
        object? newValues,
        CancellationToken ct = default);
}
