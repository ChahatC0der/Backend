using FluentValidation;

public class BulkUpdateClassGroupCommandValidator : AbstractValidator<BulkUpdateClassGroupCommand>
{
    public BulkUpdateClassGroupCommandValidator()
    {
        RuleFor(x => x.Request.Ids).NotEmpty();
        RuleFor(x => x.Request.Name).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Request.Sequence).GreaterThan((byte)0);
        RuleFor(x => x.Request.Description).MaximumLength(255).When(x => x.Request.Description != null);
    }
}