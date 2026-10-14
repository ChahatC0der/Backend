using FluentValidation;

namespace SchoolERP.Application.Features.Student.Commands.RegistrationDocument.CreateRegistrationDocument;

public class CreateRegistrationDocumentCommandValidator : AbstractValidator<CreateRegistrationDocumentCommand>
{
    public CreateRegistrationDocumentCommandValidator()
    {
        RuleFor(x => x.Request.RegistrationId).GreaterThan(0);
        RuleFor(x => x.Request.DocumentType).NotEmpty().MaximumLength(50)
            .Must(t => new[] { "birth_certificate", "marksheet", "tc", "aadhar", "photo", "medical", "other" }.Contains(t));
    }
}
