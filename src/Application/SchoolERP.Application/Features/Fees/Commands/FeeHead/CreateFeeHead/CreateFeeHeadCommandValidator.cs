using FluentValidation;

namespace SchoolERP.Application.Features.Fees.Commands.FeeHead.CreateFeeHead;

public sealed class CreateFeeHeadCommandValidator : AbstractValidator<CreateFeeHeadCommand>
{
    public CreateFeeHeadCommandValidator()
    {
        RuleFor(x => x.Request.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Request.Code).NotEmpty().MaximumLength(50)
            .Matches("^[A-Z0-9_-]+$")
            .WithMessage("Code must be uppercase letters/digits/underscores/hyphens only.");
        RuleFor(x => x.Request.Description).MaximumLength(500);
    }
}
