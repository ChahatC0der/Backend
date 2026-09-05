using FluentValidation;

public class BulkPatchAcademicYearCommandValidator : AbstractValidator<BulkPatchAcademicYearCommand>
{
    public BulkPatchAcademicYearCommandValidator()
    {
        RuleFor(x => x.Request.Ids).NotEmpty();
        RuleFor(x => x.Request.Name).MaximumLength(20).When(x => !string.IsNullOrEmpty(x.Request.Name));
        RuleFor(x => x.Request.EndDate)
            .GreaterThanOrEqualTo(x => x.Request.StartDate)
            .When(x => x.Request.StartDate.HasValue && x.Request.EndDate.HasValue);
        RuleFor(x => x.Request.Status)
            .Must(s => new[] { "upcoming", "active", "closed" }.Contains(s))
            .When(x => !string.IsNullOrEmpty(x.Request.Status));
    }
}