using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Fees.DTOs.Invoice;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Fees.Commands.Invoice.AddManualInvoiceLine;

public record AddManualInvoiceLineCommand(long InvoiceId, AddManualLineRequest Request) : ICommand<InvoiceResponse>;
