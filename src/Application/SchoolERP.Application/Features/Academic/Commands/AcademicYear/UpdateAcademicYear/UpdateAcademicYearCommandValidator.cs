using FluentValidation;

public class UpdateAcademicYearCommandValidator : AbstractValidator<UpdateAcademicYearCommand>
{
    public UpdateAcademicYearCommandValidator()
    {
        RuleFor(x => x.Request.Id).GreaterThan(0);
        RuleFor(x => x.Request.Name).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Request.StartDate).NotEmpty();
        RuleFor(x => x.Request.EndDate).NotEmpty().GreaterThanOrEqualTo(x => x.Request.StartDate);
        RuleFor(x => x.Request.Status).Must(s => new[] { "upcoming", "active", "closed" }.Contains(s));
    }
}