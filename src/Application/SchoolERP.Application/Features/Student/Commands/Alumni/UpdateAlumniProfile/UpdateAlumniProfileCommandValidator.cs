using FluentValidation;

namespace SchoolERP.Application.Features.Student.Commands.Alumni.UpdateAlumniProfile;

public class UpdateAlumniProfileCommandValidator : AbstractValidator<UpdateAlumniProfileCommand>
{
    public UpdateAlumniProfileCommandValidator()
    {
        RuleFor(x => x.Request.Id).GreaterThan(0);
        RuleFor(x => x.Request.ContactEmail).EmailAddress().When(x => !string.IsNullOrEmpty(x.Request.ContactEmail));
    }
}
