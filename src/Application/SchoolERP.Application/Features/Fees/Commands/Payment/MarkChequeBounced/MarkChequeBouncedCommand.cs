using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Fees.DTOs.Payment;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Fees.Commands.Payment.MarkChequeBounced;

public record MarkChequeBouncedCommand(long PaymentId, string Reason) : ICommand<PaymentResponse>;
