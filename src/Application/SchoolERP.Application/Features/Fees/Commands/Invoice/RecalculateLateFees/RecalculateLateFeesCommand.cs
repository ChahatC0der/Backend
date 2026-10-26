using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Fees.DTOs.Invoice;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Fees.Commands.Invoice.RecalculateLateFees;

public record RecalculateLateFeesCommand(RecalculateLateFeesRequest Request) : ICommand<RecalculateLateFeesResult>;

public sealed record RecalculateLateFeesResult(
    int InvoicesScanned,
    int InvoicesUpdated,
    int InvoicesSkipped,
    decimal TotalLateFeeAdded,
    bool DryRun);
