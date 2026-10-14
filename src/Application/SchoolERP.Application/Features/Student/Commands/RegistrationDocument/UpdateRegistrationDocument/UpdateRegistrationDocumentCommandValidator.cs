using FluentValidation;

namespace SchoolERP.Application.Features.Student.Commands.RegistrationDocument.UpdateRegistrationDocument;

public class UpdateRegistrationDocumentCommandValidator : AbstractValidator<UpdateRegistrationDocumentCommand>
{
    public UpdateRegistrationDocumentCommandValidator()
    {
        RuleFor(x => x.Request.Id).GreaterThan(0);
        RuleFor(x => x.Request.DocumentType).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Request.Status).NotEmpty()
            .Must(s => new[] { "not_uploaded", "uploaded", "verified", "rejected" }.Contains(s));
    }
}
