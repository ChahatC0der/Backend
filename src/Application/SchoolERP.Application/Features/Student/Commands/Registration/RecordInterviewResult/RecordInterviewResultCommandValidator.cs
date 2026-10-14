using FluentValidation;

namespace SchoolERP.Application.Features.Student.Commands.Registration.RecordInterviewResult;

public class RecordInterviewResultCommandValidator : AbstractValidator<RecordInterviewResultCommand>
{
    public RecordInterviewResultCommandValidator()
    {
        RuleFor(x => x.Request.Id).GreaterThan(0);
        RuleFor(x => x.Request.InterviewResult).NotEmpty()
            .Must(r => new[] { "pass", "fail" }.Contains(r));
    }
}
