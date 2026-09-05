using FluentValidation;

public class PatchMasterItemCommandValidator : AbstractValidator<PatchMasterItemCommand>
{
    public PatchMasterItemCommandValidator()
    {
        RuleFor(x => x.Request.Id).GreaterThan(0);
        RuleFor(x => x.Request.CategoryId).GreaterThan(0).When(x => x.Request.CategoryId.HasValue);
        RuleFor(x => x.Request.Value).MaximumLength(255).When(x => !string.IsNullOrEmpty(x.Request.Value));
        RuleFor(x => x.Request.Code).MaximumLength(50).When(x => !string.IsNullOrEmpty(x.Request.Code));
        RuleFor(x => x.Request.Description).MaximumLength(255).When(x => !string.IsNullOrEmpty(x.Request.Description));
        RuleFor(x => x.Request.SortOrder).GreaterThanOrEqualTo(0).When(x => x.Request.SortOrder.HasValue);
    }
}