using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Fees.DTOs.Payment;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Fees.Commands.Payment.CollectPayment;

public record CollectPaymentCommand(CollectPaymentRequest Request) : ICommand<PaymentResponse>;
