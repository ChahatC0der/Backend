using FluentValidation;

public class BulkUpdateAcademicYearCommandValidator : AbstractValidator<BulkUpdateAcademicYearCommand>
{
    public BulkUpdateAcademicYearCommandValidator()
    {
        RuleFor(x => x.Request.Ids).NotEmpty();
        RuleFor(x => x.Request.Name).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Request.StartDate).NotEmpty();
        RuleFor(x => x.Request.EndDate).NotEmpty().GreaterThanOrEqualTo(x => x.Request.StartDate);
        RuleFor(x => x.Request.Status).Must(s => new[] { "upcoming", "active", "closed" }.Contains(s));
    }
}