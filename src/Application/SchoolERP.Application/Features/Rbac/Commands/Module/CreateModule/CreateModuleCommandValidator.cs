using FluentValidation;

namespace SchoolERP.Application.Features.Rbac.Commands.Module.CreateModule;

public class CreateModuleCommandValidator : AbstractValidator<CreateModuleCommand>
{
    public CreateModuleCommandValidator()
    {
        RuleFor(x => x.Request.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Request.Key)
            .NotEmpty().MaximumLength(100)
            .Matches("^[a-z0-9_]+$").WithMessage("Key must be lowercase letters, numbers, underscore.");
        RuleFor(x => x.Request.Description).MaximumLength(255).When(x => x.Request.Description != null);
    }
}