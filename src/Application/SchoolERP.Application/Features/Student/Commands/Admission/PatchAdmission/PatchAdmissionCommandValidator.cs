using FluentValidation;

namespace SchoolERP.Application.Features.Student.Commands.Admission.PatchAdmission;

public class PatchAdmissionCommandValidator : AbstractValidator<PatchAdmissionCommand>
{
    public PatchAdmissionCommandValidator()
    {
        RuleFor(x => x.Request.Id).GreaterThan(0);
    }
}
