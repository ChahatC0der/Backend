using FluentValidation;

namespace SchoolERP.Application.Features.Fees.Commands.Concession.CreateConcession;

public sealed class CreateConcessionCommandValidator : AbstractValidator<CreateConcessionCommand>
{
    public CreateConcessionCommandValidator()
    {
        RuleFor(x => x.Request.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Request.Type).Must(t => t is "percentage" or "fixed" or "sibling" or "merit")
            .WithMessage("Type must be one of: percentage, fixed, sibling, merit.");
        RuleFor(x => x.Request.Value).GreaterThan(0);
        RuleFor(x => x.Request.EndDate)
            .GreaterThanOrEqualTo(x => x.Request.StartDate)
            .When(x => x.Request.StartDate.HasValue && x.Request.EndDate.HasValue);
    }
}
