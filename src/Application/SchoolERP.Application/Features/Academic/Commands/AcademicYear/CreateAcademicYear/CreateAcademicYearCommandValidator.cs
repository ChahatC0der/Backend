using FluentValidation;

namespace SchoolERP.Application.Features.Academic.Commands.AcademicYear.CreateAcademicYear;

public class CreateAcademicYearCommandValidator : AbstractValidator<CreateAcademicYearCommand>
{
    public CreateAcademicYearCommandValidator()
    {
        RuleFor(x => x.Request.Name).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Request.StartDate).NotEmpty();
        RuleFor(x => x.Request.EndDate).NotEmpty().GreaterThanOrEqualTo(x => x.Request.StartDate);
    }
}   