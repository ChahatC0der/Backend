using FluentValidation;

namespace SchoolERP.Application.Features.Rbac.Commands.Role.CreateRole;

public class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleCommandValidator()
    {
        RuleFor(x => x.Request.Name)
            .NotEmpty().WithMessage("Role name is required.")
            .MaximumLength(100);

        RuleFor(x => x.Request.Code)
            .NotEmpty().WithMessage("Role code is required.")
            .MaximumLength(100)
            .Matches("^[A-Z0-9_]+$").WithMessage("Role code must be uppercase letters, numbers, or underscore.");

        RuleFor(x => x.Request.PermissionIds)
            .Must(ids => ids == null || ids.Count <= 50)
            .WithMessage("Maximum 50 permissions allowed.");
    }
}