using FluentValidation;

public class PatchSectionCommandValidator : AbstractValidator<PatchSectionCommand>
{
    public PatchSectionCommandValidator()
    {
        RuleFor(x => x.Request.Id).GreaterThan(0);
        RuleFor(x => x.Request.Name).MaximumLength(10).When(x => !string.IsNullOrEmpty(x.Request.Name));
        RuleFor(x => x.Request.Capacity).GreaterThan(0).When(x => x.Request.Capacity.HasValue);
        RuleFor(x => x.Request.ClassId).GreaterThan(0).When(x => x.Request.ClassId.HasValue);
    }
}