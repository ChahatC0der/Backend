using FluentValidation;

namespace SchoolERP.Application.Features.Academic.Commands.Class.CreateClass;

public class CreateClassCommandValidator : AbstractValidator<CreateClassCommand>
{
    public CreateClassCommandValidator()
    {
        RuleFor(x => x.Request.Name).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Request.Sequence).GreaterThan((byte)0);
        RuleFor(x => x.Request.ClassGroupId).GreaterThan(0).When(x => x.Request.ClassGroupId.HasValue);
    }
}