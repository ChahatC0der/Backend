using FluentValidation;

namespace SchoolERP.Application.Features.Student.Commands.Registration.CreateRegistration;

public class CreateRegistrationCommandValidator : AbstractValidator<CreateRegistrationCommand>
{
    public CreateRegistrationCommandValidator()
    {
        RuleFor(x => x.Request.AcademicYearId).GreaterThan(0);
        RuleFor(x => x.Request.ClassId).GreaterThan(0);
        RuleFor(x => x.Request.StudentFirstName).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Request.ParentPrimaryMobile).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Request.ParentPrimaryEmail).EmailAddress().When(x => !string.IsNullOrEmpty(x.Request.ParentPrimaryEmail));
        RuleFor(x => x.Request.FatherEmail).EmailAddress().When(x => !string.IsNullOrEmpty(x.Request.FatherEmail));
        RuleFor(x => x.Request.MotherEmail).EmailAddress().When(x => !string.IsNullOrEmpty(x.Request.MotherEmail));
    }
}
