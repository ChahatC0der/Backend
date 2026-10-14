using FluentValidation;

namespace SchoolERP.Application.Features.Student.Commands.Admission.CreateAdmission;

public class CreateAdmissionCommandValidator : AbstractValidator<CreateAdmissionCommand>
{
    public CreateAdmissionCommandValidator()
    {
        RuleFor(x => x.Request.AcademicYearId).GreaterThan(0);
        RuleFor(x => x.Request.ClassId).GreaterThan(0);
        RuleFor(x => x.Request.SectionId).GreaterThan(0);
    }
}
