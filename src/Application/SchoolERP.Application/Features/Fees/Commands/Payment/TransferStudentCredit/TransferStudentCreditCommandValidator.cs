using FluentValidation;

namespace SchoolERP.Application.Features.Fees.Commands.Payment.TransferStudentCredit;

public sealed class TransferStudentCreditCommandValidator : AbstractValidator<TransferStudentCreditCommand>
{
    public TransferStudentCreditCommandValidator()
    {
        RuleFor(x => x.Request.FromStudentId).GreaterThan(0);
        RuleFor(x => x.Request.ToStudentId).GreaterThan(0);
        RuleFor(x => x.Request.Amount).GreaterThan(0);
        RuleFor(x => x.Request.Reason).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Request).Must(r => r.FromStudentId != r.ToStudentId)
            .WithMessage("Source and destination students must be different.");
    }
}
