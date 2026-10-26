using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Fees.DTOs.Invoice;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Fees.Queries.Invoice.GetInvoiceById;

public sealed record GetInvoiceByIdQuery(long Id) : IQuery<InvoiceResponse>;
