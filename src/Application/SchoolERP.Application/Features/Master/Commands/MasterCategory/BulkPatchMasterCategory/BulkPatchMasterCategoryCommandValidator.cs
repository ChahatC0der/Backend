using FluentValidation;

public class BulkPatchMasterCategoryCommandValidator : AbstractValidator<BulkPatchMasterCategoryCommand>
{
    public BulkPatchMasterCategoryCommandValidator()
    {
        RuleFor(x => x.Request.Ids).NotEmpty();
        RuleFor(x => x.Request.ModuleId).GreaterThan(0).When(x => x.Request.ModuleId.HasValue);
        RuleFor(x => x.Request.Key).MaximumLength(50).When(x => !string.IsNullOrEmpty(x.Request.Key));
        RuleFor(x => x.Request.Name).MaximumLength(100).When(x => !string.IsNullOrEmpty(x.Request.Name));
        RuleFor(x => x.Request.Description).MaximumLength(255).When(x => !string.IsNullOrEmpty(x.Request.Description));
    }
}