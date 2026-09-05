using FluentValidation;

public class PatchClassGroupCommandValidator : AbstractValidator<PatchClassGroupCommand>
{
    public PatchClassGroupCommandValidator()
    {
        RuleFor(x => x.Request.Id).GreaterThan(0);
        RuleFor(x => x.Request.Name).MaximumLength(50).When(x => !string.IsNullOrEmpty(x.Request.Name));
        RuleFor(x => x.Request.Description).MaximumLength(255).When(x => !string.IsNullOrEmpty(x.Request.Description));
    }
}