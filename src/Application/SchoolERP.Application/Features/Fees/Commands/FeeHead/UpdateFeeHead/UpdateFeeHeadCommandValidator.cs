using FluentValidation;

namespace SchoolERP.Application.Features.Fees.Commands.FeeHead.UpdateFeeHead;

public sealed class UpdateFeeHeadCommandValidator : AbstractValidator<UpdateFeeHeadCommand>
{
    public UpdateFeeHeadCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Request.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Request.Code).NotEmpty().MaximumLength(20)
            .Matches("^[A-Z0-9_-]+$");
        RuleFor(x => x.Request.Description).MaximumLength(500);
    }
}
