using FluentValidation;

namespace SchoolERP.Application.Features.Fees.Commands.Concession.RejectConcession;

public sealed class RejectConcessionCommandValidator : AbstractValidator<RejectConcessionCommand>
{
    public RejectConcessionCommandValidator()
    {
        RuleFor(x => x.AllocationId).GreaterThan(0);
        RuleFor(x => x.Request.RejectionReason).NotEmpty().MaximumLength(500);
    }
}
