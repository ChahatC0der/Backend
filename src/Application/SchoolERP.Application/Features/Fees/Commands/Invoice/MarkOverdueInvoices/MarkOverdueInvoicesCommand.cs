using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Fees.Commands.Invoice.MarkOverdueInvoices;

/// <summary>
/// Flips Sent / PartiallyPaid invoices to Overdue when DueDate < UtcNow and outstanding > 0.
/// Idempotent  already-Overdue invoices are not touched.
/// Returns count of invoices flipped.
///
/// Optional AcademicYearId scopes the operation to one AY; null = all AYs in the branch.
/// Same handler is called by OverdueInvoiceStatusJob (scheduled daily at 02:30 UTC).
/// </summary>
public record MarkOverdueInvoicesCommand(long? AcademicYearId = null) : ICommand<int>;
