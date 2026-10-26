using FluentValidation;

namespace SchoolERP.Application.Features.Fees.Commands.Refund.ApproveRefund;

public sealed class ApproveRefundCommandValidator : AbstractValidator<ApproveRefundCommand>
{
    public ApproveRefundCommandValidator() => RuleFor(x => x.RefundId).GreaterThan(0);
}
