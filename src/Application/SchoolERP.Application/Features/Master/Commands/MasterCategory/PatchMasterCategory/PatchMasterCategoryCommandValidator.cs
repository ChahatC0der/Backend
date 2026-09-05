using FluentValidation;

public class PatchMasterCategoryCommandValidator : AbstractValidator<PatchMasterCategoryCommand>
{
    public PatchMasterCategoryCommandValidator()
    {
        RuleFor(x => x.Request.Id).GreaterThan(0);
        RuleFor(x => x.Request.ModuleId).GreaterThan(0).When(x => x.Request.ModuleId.HasValue);
        RuleFor(x => x.Request.Key).MaximumLength(50).When(x => !string.IsNullOrEmpty(x.Request.Key));
        RuleFor(x => x.Request.Name).MaximumLength(100).When(x => !string.IsNullOrEmpty(x.Request.Name));
        RuleFor(x => x.Request.Description).MaximumLength(255).When(x => !string.IsNullOrEmpty(x.Request.Description));
    }
}