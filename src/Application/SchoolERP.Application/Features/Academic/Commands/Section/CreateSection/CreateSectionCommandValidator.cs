using FluentValidation;

namespace SchoolERP.Application.Features.Academic.Commands.Section.CreateSection;

public class CreateSectionCommandValidator : AbstractValidator<CreateSectionCommand>
{
    public CreateSectionCommandValidator()
    {
        RuleFor(x => x.Request.ClassId).GreaterThan(0);
        RuleFor(x => x.Request.Name).NotEmpty().MaximumLength(10);
        RuleFor(x => x.Request.Capacity).GreaterThan(0).When(x => x.Request.Capacity.HasValue);
    }
}