using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Fees.DTOs.Invoice;
using SchoolERP.Application.Features.Fees.DTOs.Portal;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Fees.Commands.Portal.OptInOptionalFee;

public record OptInOptionalFeeCommand(OptInOptionalFeeRequest Request) : ICommand<InvoiceResponse>;
