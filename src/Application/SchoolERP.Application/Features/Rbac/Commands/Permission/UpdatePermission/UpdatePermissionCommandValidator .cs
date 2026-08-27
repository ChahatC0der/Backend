using FluentValidation;

public class UpdatePermissionCommandValidator : AbstractValidator<UpdatePermissionCommand>
{
    public UpdatePermissionCommandValidator()
    {
        RuleFor(x => x.Request.Id).GreaterThan(0);
        RuleFor(x => x.Request.ModuleId).GreaterThan(0);
        RuleFor(x => x.Request.Action).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Request.Key).NotEmpty().MaximumLength(255);
    }
}