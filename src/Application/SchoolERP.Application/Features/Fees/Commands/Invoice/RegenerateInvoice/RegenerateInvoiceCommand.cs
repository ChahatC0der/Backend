using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Fees.DTOs.Invoice;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Fees.Commands.Invoice.RegenerateInvoice;

public record RegenerateInvoiceCommand(long InvoiceId) : ICommand<InvoiceResponse>;
