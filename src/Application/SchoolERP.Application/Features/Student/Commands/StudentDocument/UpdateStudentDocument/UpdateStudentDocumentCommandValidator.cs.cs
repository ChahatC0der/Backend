using FluentValidation;

public class UpdateStudentDocumentCommandValidator : AbstractValidator<UpdateStudentDocumentCommand>
{
    public UpdateStudentDocumentCommandValidator()
    {
        RuleFor(x => x.Request.Id).GreaterThan(0);
        RuleFor(x => x.Request.StudentId).GreaterThan(0);
        RuleFor(x => x.Request.DocumentType).NotEmpty()
            .Must(t => new[] { "birth_certificate", "aadhar", "marksheet", "medical", "photo", "other" }.Contains(t));
        RuleFor(x => x.Request.FileUrl).NotEmpty().MaximumLength(500);
    }
}