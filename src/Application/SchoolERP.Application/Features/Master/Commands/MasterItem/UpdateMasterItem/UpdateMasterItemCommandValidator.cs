using FluentValidation;

public class UpdateMasterItemCommandValidator : AbstractValidator<UpdateMasterItemCommand>
{
    public UpdateMasterItemCommandValidator()
    {
        RuleFor(x => x.Request.Id).GreaterThan(0);
        RuleFor(x => x.Request.CategoryId).GreaterThan(0);
        RuleFor(x => x.Request.Value).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Request.Code).MaximumLength(50).When(x => !string.IsNullOrEmpty(x.Request.Code));
        RuleFor(x => x.Request.Description).MaximumLength(255).When(x => !string.IsNullOrEmpty(x.Request.Description));
        RuleFor(x => x.Request.SortOrder).GreaterThanOrEqualTo(0);
    }
}