using FluentValidation;

namespace SchoolERP.Application.Features.Student.Commands.RegistrationDocument.VerifyRegistrationDocument;

public class VerifyRegistrationDocumentCommandValidator : AbstractValidator<VerifyRegistrationDocumentCommand>
{
    public VerifyRegistrationDocumentCommandValidator()
    {
        RuleFor(x => x.Request.Id).GreaterThan(0);
        RuleFor(x => x.Request.RejectionReason)
            .NotEmpty().MaximumLength(1000)
            .When(x => !x.Request.Verified);
    }
}
