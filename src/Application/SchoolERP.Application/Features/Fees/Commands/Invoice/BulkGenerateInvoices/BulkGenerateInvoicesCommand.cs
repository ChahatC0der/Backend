using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Fees.DTOs.Invoice;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Fees.Commands.Invoice.BulkGenerateInvoices;

public record BulkGenerateInvoicesCommand(BulkGenerateInvoicesRequest Request) : ICommand<BulkGenerateResult>;

public sealed record BulkGenerateResult(int Targeted, int Generated, int Skipped, int Failed, IReadOnlyList<string> Errors);
