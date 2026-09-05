using FluentValidation;

public class BulkUpdateSectionCommandValidator : AbstractValidator<BulkUpdateSectionCommand>
{
    public BulkUpdateSectionCommandValidator()
    {
        RuleFor(x => x.Request.Ids).NotEmpty();
        RuleFor(x => x.Request.ClassId).GreaterThan(0);
        RuleFor(x => x.Request.Name).NotEmpty().MaximumLength(10);
        RuleFor(x => x.Request.Capacity).GreaterThan(0).When(x => x.Request.Capacity.HasValue);
    }
}