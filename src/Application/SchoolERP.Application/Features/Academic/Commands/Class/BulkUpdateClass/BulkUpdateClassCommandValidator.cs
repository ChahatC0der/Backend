using FluentValidation;

public class BulkUpdateClassCommandValidator : AbstractValidator<BulkUpdateClassCommand>
{
    public BulkUpdateClassCommandValidator()
    {
        RuleFor(x => x.Request.Ids).NotEmpty();
        RuleFor(x => x.Request.Name).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Request.Sequence).GreaterThan((byte)0);
        RuleFor(x => x.Request.ClassGroupId).GreaterThan(0).When(x => x.Request.ClassGroupId.HasValue);
    }
}