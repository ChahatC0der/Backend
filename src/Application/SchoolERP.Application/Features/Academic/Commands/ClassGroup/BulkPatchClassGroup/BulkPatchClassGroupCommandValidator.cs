using FluentValidation;

public class BulkPatchClassGroupCommandValidator : AbstractValidator<BulkPatchClassGroupCommand>
{
    public BulkPatchClassGroupCommandValidator()
    {
        RuleFor(x => x.Request.Ids).NotEmpty();
        RuleFor(x => x.Request.Name).MaximumLength(50).When(x => !string.IsNullOrEmpty(x.Request.Name));
        RuleFor(x => x.Request.Description).MaximumLength(255).When(x => !string.IsNullOrEmpty(x.Request.Description));
    }
}