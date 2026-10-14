using FluentValidation;

namespace SchoolERP.Application.Features.Student.Commands.Admission.CancelAdmission;

public class CancelAdmissionCommandValidator : AbstractValidator<CancelAdmissionCommand>
{
    public CancelAdmissionCommandValidator()
    {
        RuleFor(x => x.Request.Id).GreaterThan(0);
        RuleFor(x => x.Request.Reason).NotEmpty().MaximumLength(500);
    }
}
