using FluentValidation;

public class PatchAcademicYearCommandValidator : AbstractValidator<PatchAcademicYearCommand>
{
    public PatchAcademicYearCommandValidator()
    {
        RuleFor(x => x.Request.Id).GreaterThan(0);
        RuleFor(x => x.Request.Name).MaximumLength(20).When(x => !string.IsNullOrEmpty(x.Request.Name));
        RuleFor(x => x.Request.EndDate)
            .GreaterThanOrEqualTo(x => x.Request.StartDate)
            .When(x => x.Request.StartDate.HasValue && x.Request.EndDate.HasValue);
        RuleFor(x => x.Request.Status)
            .Must(s => new[] { "upcoming", "active", "closed" }.Contains(s))
            .When(x => !string.IsNullOrEmpty(x.Request.Status));
    }
}