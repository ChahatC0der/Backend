using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Fees.DTOs.Refund;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Fees.Commands.Refund.RequestRefund;

public record RequestRefundCommand(RequestRefundRequest Request) : ICommand<RefundResponse>;
