using FluentValidation;

namespace SchoolERP.Application.Features.Student.Commands.Student.BulkPatchStudent;

public class BulkPatchStudentCommandValidator : AbstractValidator<BulkPatchStudentCommand>
{
    public BulkPatchStudentCommandValidator()
    {
        RuleFor(x => x.Request.Ids).NotEmpty();
        RuleFor(x => x.Request.Status)
            .Must(s => new[] { "registered", "enrolled", "active", "graduated", "dropped", "blocked", "transferred_out" }.Contains(s))
            .When(x => !string.IsNullOrEmpty(x.Request.Status));
    }
}
