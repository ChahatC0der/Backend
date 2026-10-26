using FluentValidation;

namespace SchoolERP.Application.Features.Fees.Commands.Payment.MarkChequeBounced;

public sealed class MarkChequeBouncedCommandValidator : AbstractValidator<MarkChequeBouncedCommand>
{
    public MarkChequeBouncedCommandValidator()
    {
        RuleFor(x => x.PaymentId).GreaterThan(0);
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}
