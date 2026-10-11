using FluentValidation;

public class PatchModuleCommandValidator : AbstractValidator<PatchModuleCommand>
{
    public PatchModuleCommandValidator()
    {
        RuleFor(x => x.Request.Id).GreaterThan(0);
        RuleFor(x => x.Request.Name).MaximumLength(100).When(x => !string.IsNullOrEmpty(x.Request.Name));
        RuleFor(x => x.Request.Key).MaximumLength(100).Matches("^[a-z0-9_]+$").When(x => !string.IsNullOrEmpty(x.Request.Key));
    }
}