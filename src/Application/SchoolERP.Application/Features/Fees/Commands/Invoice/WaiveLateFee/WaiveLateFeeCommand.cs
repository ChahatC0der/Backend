using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Fees.DTOs.Invoice;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Fees.Commands.Invoice.WaiveLateFee;

public record WaiveLateFeeCommand(long InvoiceId, WaiveLateFeeRequest Request) : ICommand<InvoiceResponse>;
