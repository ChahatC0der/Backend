using FluentValidation;

namespace SchoolERP.Application.Features.Student.Commands.EnquiryFollowup.PatchEnquiryFollowup;

public class PatchEnquiryFollowupCommandValidator : AbstractValidator<PatchEnquiryFollowupCommand>
{
    public PatchEnquiryFollowupCommandValidator()
    {
        RuleFor(x => x.Request.Id).GreaterThan(0);
        RuleFor(x => x.Request.ActivityType)
            .Must(t => new[] { "call", "sms", "email", "walkin", "other" }.Contains(t))
            .When(x => !string.IsNullOrEmpty(x.Request.ActivityType));
    }
}
