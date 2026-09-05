using FluentValidation;

public class BulkUpdateMasterCategoryCommandValidator : AbstractValidator<BulkUpdateMasterCategoryCommand>
{
    public BulkUpdateMasterCategoryCommandValidator()
    {
        RuleFor(x => x.Request.Ids).NotEmpty();
        RuleFor(x => x.Request.ModuleId).GreaterThan(0);
        RuleFor(x => x.Request.Key).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Request.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Request.Description).MaximumLength(255).When(x => x.Request.Description != null);
    }
}