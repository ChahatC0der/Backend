using FluentValidation;

namespace SchoolERP.Application.Features.Student.Commands.Student.BulkUpdateStudent;

public class BulkUpdateStudentCommandValidator : AbstractValidator<BulkUpdateStudentCommand>
{
    public BulkUpdateStudentCommandValidator()
    {
        RuleFor(x => x.Request.Ids).NotEmpty();
        RuleFor(x => x.Request.Status).NotEmpty()
            .Must(s => new[] { "registered", "enrolled", "active", "graduated", "dropped", "blocked", "transferred_out" }.Contains(s));
    }
}
