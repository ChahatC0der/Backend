using FluentValidation;

namespace SchoolERP.Application.Features.Student.Commands.EnquiryFollowup.UpdateEnquiryFollowup;

public class UpdateEnquiryFollowupCommandValidator : AbstractValidator<UpdateEnquiryFollowupCommand>
{
    public UpdateEnquiryFollowupCommandValidator()
    {
        RuleFor(x => x.Request.Id).GreaterThan(0);
        RuleFor(x => x.Request.EnquiryId).GreaterThan(0);
        RuleFor(x => x.Request.ActivityType).NotEmpty().MaximumLength(20)
            .Must(t => new[] { "call", "sms", "email", "walkin", "other" }.Contains(t));
    }
}
