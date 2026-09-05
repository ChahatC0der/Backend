using FluentValidation;

public class PatchClassCommandValidator : AbstractValidator<PatchClassCommand>
{
    public PatchClassCommandValidator()
    {
        RuleFor(x => x.Request.Id).GreaterThan(0);
        RuleFor(x => x.Request.Name).MaximumLength(50).When(x => !string.IsNullOrEmpty(x.Request.Name));
        RuleFor(x => x.Request.ClassGroupId).GreaterThan(0).When(x => x.Request.ClassGroupId.HasValue);
    }
}