using FluentValidation;

public class UpdateMasterCategoryCommandValidator : AbstractValidator<UpdateMasterCategoryCommand>
{
    public UpdateMasterCategoryCommandValidator()
    {
        RuleFor(x => x.Request.Id).GreaterThan(0);
        RuleFor(x => x.Request.ModuleId).GreaterThan(0);
        RuleFor(x => x.Request.Key).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Request.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Request.Description).MaximumLength(255).When(x => x.Request.Description != null);
    }
}