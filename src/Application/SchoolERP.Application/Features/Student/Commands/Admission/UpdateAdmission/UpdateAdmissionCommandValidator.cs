using FluentValidation;

namespace SchoolERP.Application.Features.Student.Commands.Admission.UpdateAdmission;

public class UpdateAdmissionCommandValidator : AbstractValidator<UpdateAdmissionCommand>
{
    public UpdateAdmissionCommandValidator()
    {
        RuleFor(x => x.Request.Id).GreaterThan(0);
        RuleFor(x => x.Request.AcademicYearId).GreaterThan(0);
        RuleFor(x => x.Request.ClassId).GreaterThan(0);
        RuleFor(x => x.Request.SectionId).GreaterThan(0);
    }
}
