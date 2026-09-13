using FluentValidation;

public class PatchStudentDocumentCommandValidator : AbstractValidator<PatchStudentDocumentCommand>
{
    public PatchStudentDocumentCommandValidator()
    {
        RuleFor(x => x.Request.Id).GreaterThan(0);
        RuleFor(x => x.Request.DocumentType)
            .Must(t => new[] { "birth_certificate", "aadhar", "marksheet", "medical", "photo", "other" }.Contains(t))
            .When(x => !string.IsNullOrEmpty(x.Request.DocumentType));
        RuleFor(x => x.Request.FileUrl).MaximumLength(500).When(x => !string.IsNullOrEmpty(x.Request.FileUrl));
    }
}