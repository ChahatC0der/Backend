using FluentValidation;

public class BulkUpdateStudentDocumentCommandValidator : AbstractValidator<BulkUpdateStudentDocumentCommand>
{
    public BulkUpdateStudentDocumentCommandValidator()
    {
        RuleFor(x => x.Request.Ids).NotEmpty();
    }
}