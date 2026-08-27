using FluentValidation;
using SchoolERP.Application.Features.Rbac.Commands.Permission.CreatePermission;

public class CreatePermissionCommandValidator : AbstractValidator<CreatePermissionCommand>
{
    public CreatePermissionCommandValidator()
    {
        RuleFor(x => x.Request.ModuleId).GreaterThan(0);
        RuleFor(x => x.Request.Action).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Request.Key).NotEmpty().MaximumLength(255);
    }
}