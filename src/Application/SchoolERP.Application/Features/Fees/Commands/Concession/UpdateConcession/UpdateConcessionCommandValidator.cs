using FluentValidation;

namespace SchoolERP.Application.Features.Fees.Commands.Concession.UpdateConcession;

public sealed class UpdateConcessionCommandValidator : AbstractValidator<UpdateConcessionCommand>
{
    public UpdateConcessionCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Request.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Request.Type).Must(t => t is "percentage" or "fixed" or "sibling" or "merit");
        RuleFor(x => x.Request.Value).GreaterThan(0);
    }
}
