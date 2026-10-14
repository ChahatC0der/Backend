using FluentValidation;

namespace SchoolERP.Application.Features.Student.Commands.Registration.UpdateRegistration;

public class UpdateRegistrationCommandValidator : AbstractValidator<UpdateRegistrationCommand>
{
    public UpdateRegistrationCommandValidator()
    {
        RuleFor(x => x.Request.Id).GreaterThan(0);
        RuleFor(x => x.Request.AcademicYearId).GreaterThan(0);
        RuleFor(x => x.Request.ClassId).GreaterThan(0);
        RuleFor(x => x.Request.StudentFirstName).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Request.ParentPrimaryMobile).NotEmpty().MaximumLength(20);
    }
}
