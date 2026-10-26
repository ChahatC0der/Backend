using FluentValidation;

namespace SchoolERP.Application.Features.Fees.Commands.Refund.CompleteRefund;

public sealed class CompleteRefundCommandValidator : AbstractValidator<CompleteRefundCommand>
{
    public CompleteRefundCommandValidator() => RuleFor(x => x.RefundId).GreaterThan(0);
}
