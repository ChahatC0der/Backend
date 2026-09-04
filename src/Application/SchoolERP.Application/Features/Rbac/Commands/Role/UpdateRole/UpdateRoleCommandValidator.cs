using FluentValidation;

public class UpdateRoleCommandValidator : AbstractValidator<UpdateRoleCommand>
{
    public UpdateRoleCommandValidator()
    {
        RuleFor(x => x.id).GreaterThan(0).WithMessage("Role id is required.");
        RuleFor(x => x.Request.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Request.Code).NotEmpty().MaximumLength(100).Matches("^[A-Z0-9_]+$");
        RuleFor(x => x.Request.PermissionIds).Must(ids => ids == null || ids.Count <= 50);
    }
}