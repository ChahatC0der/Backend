using FluentValidation;

namespace SchoolERP.Application.Features.Student.Commands.StudentDocument.CreateStudentDocument;

public class CreateStudentDocumentCommandValidator : AbstractValidator<CreateStudentDocumentCommand>
{
    public CreateStudentDocumentCommandValidator()
    {
        RuleFor(x => x.Request.StudentId).GreaterThan(0);
        RuleFor(x => x.Request.DocumentType).NotEmpty()
            .Must(t => new[] { "birth_certificate", "aadhar", "marksheet", "medical", "photo", "other" }.Contains(t));
        RuleFor(x => x.Request.FileUrl).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Request.Description).MaximumLength(255).When(x => !string.IsNullOrEmpty(x.Request.Description));
    }
}