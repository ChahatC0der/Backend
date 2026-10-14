using FluentValidation;

namespace SchoolERP.Application.Features.Student.Commands.RegistrationDocument.PatchRegistrationDocument;

public class PatchRegistrationDocumentCommandValidator : AbstractValidator<PatchRegistrationDocumentCommand>
{
    public PatchRegistrationDocumentCommandValidator()
    {
        RuleFor(x => x.Request.Id).GreaterThan(0);
        RuleFor(x => x.Request.DocumentType).MaximumLength(50).When(x => !string.IsNullOrEmpty(x.Request.DocumentType));
        RuleFor(x => x.Request.Status)
            .Must(s => new[] { "not_uploaded", "uploaded", "verified", "rejected" }.Contains(s))
            .When(x => !string.IsNullOrEmpty(x.Request.Status));
    }
}
