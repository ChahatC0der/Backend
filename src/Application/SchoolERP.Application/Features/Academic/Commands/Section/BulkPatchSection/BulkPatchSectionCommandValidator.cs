using FluentValidation;

public class BulkPatchSectionCommandValidator : AbstractValidator<BulkPatchSectionCommand>
{
    public BulkPatchSectionCommandValidator()
    {
        RuleFor(x => x.Request.Ids).NotEmpty();
        RuleFor(x => x.Request.Name).MaximumLength(10).When(x => !string.IsNullOrEmpty(x.Request.Name));
        RuleFor(x => x.Request.Capacity).GreaterThan(0).When(x => x.Request.Capacity.HasValue);
        RuleFor(x => x.Request.ClassId).GreaterThan(0).When(x => x.Request.ClassId.HasValue);
    }
}