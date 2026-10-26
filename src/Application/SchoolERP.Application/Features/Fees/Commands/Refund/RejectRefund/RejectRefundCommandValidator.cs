using FluentValidation;

namespace SchoolERP.Application.Features.Fees.Commands.Refund.RejectRefund;

public sealed class RejectRefundCommandValidator : AbstractValidator<RejectRefundCommand>
{
    public RejectRefundCommandValidator()
    {
        RuleFor(x => x.RefundId).GreaterThan(0);
        RuleFor(x => x.Request.RejectionReason).NotEmpty().MaximumLength(500);
    }
}
