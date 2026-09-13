using FluentValidation;

public class BulkPatchStudentDocumentCommandValidator : AbstractValidator<BulkPatchStudentDocumentCommand>
{
    public BulkPatchStudentDocumentCommandValidator()
    {
        RuleFor(x => x.Request.Ids).NotEmpty();
    }
}