using FluentValidation;

namespace SchoolERP.Application.Features.Student.Commands.Alumni.CreateAlumni;

public class CreateAlumniCommandValidator : AbstractValidator<CreateAlumniCommand>
{
    public CreateAlumniCommandValidator()
    {
        RuleFor(x => x.Request.FirstName).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Request.GraduationDate).NotEmpty();
        RuleFor(x => x.Request.LastClassId).GreaterThan(0);
        RuleFor(x => x.Request.PassingAcademicYearId).GreaterThan(0);
        RuleFor(x => x.Request.CurrentStatus).NotEmpty()
            .Must(s => new[] { "working", "higher_studies", "self_employed", "not_employed", "retired", "deceased" }.Contains(s));
    }
}
