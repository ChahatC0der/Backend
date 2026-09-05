using FluentValidation;

public class BulkPatchClassCommandValidator : AbstractValidator<BulkPatchClassCommand>
{
    public BulkPatchClassCommandValidator()
    {
        RuleFor(x => x.Request.Ids).NotEmpty();
        RuleFor(x => x.Request.Name).MaximumLength(50).When(x => !string.IsNullOrEmpty(x.Request.Name));
        RuleFor(x => x.Request.ClassGroupId).GreaterThan(0).When(x => x.Request.ClassGroupId.HasValue);
    }
}