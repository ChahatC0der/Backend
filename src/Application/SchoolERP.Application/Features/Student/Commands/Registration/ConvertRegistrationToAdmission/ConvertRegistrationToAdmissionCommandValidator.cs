using FluentValidation;

namespace SchoolERP.Application.Features.Student.Commands.Registration.ConvertRegistrationToAdmission;

public class ConvertRegistrationToAdmissionCommandValidator : AbstractValidator<ConvertRegistrationToAdmissionCommand>
{
    public ConvertRegistrationToAdmissionCommandValidator()
    {
        RuleFor(x => x.Request.Id).GreaterThan(0);
        RuleFor(x => x.Request.AcademicYearId).GreaterThan(0);
        RuleFor(x => x.Request.ClassId).GreaterThan(0);
        RuleFor(x => x.Request.SectionId).GreaterThan(0);
    }
}
