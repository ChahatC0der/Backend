using FluentValidation;

namespace SchoolERP.Application.Features.Student.Commands.Registration.ScheduleInterview;

public class ScheduleInterviewCommandValidator : AbstractValidator<ScheduleInterviewCommand>
{
    public ScheduleInterviewCommandValidator()
    {
        RuleFor(x => x.Request.Id).GreaterThan(0);
        RuleFor(x => x.Request.InterviewDate).NotEmpty();
        RuleFor(x => x.Request.InterviewMode).NotEmpty()
            .Must(m => new[] { "online", "offline" }.Contains(m));
        RuleFor(x => x.Request.InterviewerId).GreaterThan(0);
    }
}
