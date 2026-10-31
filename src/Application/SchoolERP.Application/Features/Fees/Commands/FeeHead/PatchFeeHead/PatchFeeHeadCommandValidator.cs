using FluentValidation;

namespace SchoolERP.Application.Features.Fees.Commands.FeeHead.PatchFeeHead;

public sealed class PatchFeeHeadCommandValidator : AbstractValidator<PatchFeeHeadCommand>
{
    public PatchFeeHeadCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Request.Name).MaximumLength(150).When(x => x.Request.Name is not null);
        RuleFor(x => x.Request.Code).MaximumLength(20)
            .Matches("^[A-Z0-9_-]+$").When(x => x.Request.Code is not null);
        RuleFor(x => x.Request.Description).MaximumLength(500);
    }
}
