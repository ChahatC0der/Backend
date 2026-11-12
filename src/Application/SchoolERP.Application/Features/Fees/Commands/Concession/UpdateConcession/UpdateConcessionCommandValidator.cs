using FluentValidation;
using SchoolERP.Domain.Fees.Enums;

namespace SchoolERP.Application.Features.Fees.Commands.Concession.UpdateConcession;

public sealed class UpdateConcessionCommandValidator : AbstractValidator<UpdateConcessionCommand>
{
    public UpdateConcessionCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Request.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Request.Category).IsInEnum();
        RuleFor(x => x.Request.CalculationType).IsInEnum();
        RuleFor(x => x.Request.Value).GreaterThan(0);
        RuleFor(x => x.Request.Value)
            .LessThanOrEqualTo(100)
            .When(x => x.Request.CalculationType == ConcessionCalculationType.Percentage)
            .WithMessage("Percentage concession cannot exceed 100.");
        RuleFor(x => x.Request.EndDate)
            .GreaterThanOrEqualTo(x => x.Request.StartDate)
            .When(x => x.Request.StartDate.HasValue && x.Request.EndDate.HasValue);
    }
}