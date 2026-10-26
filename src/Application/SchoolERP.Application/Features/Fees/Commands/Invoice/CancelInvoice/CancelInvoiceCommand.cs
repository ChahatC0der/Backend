using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Fees.Commands.Invoice.CancelInvoice;

public record CancelInvoiceCommand(long InvoiceId) : ICommand<bool>;
