using FluentValidation;

namespace SchoolERP.Application.Features.Student.Commands.Enquiry.PatchEnquiry;

public class PatchEnquiryCommandValidator : AbstractValidator<PatchEnquiryCommand>
{
    public PatchEnquiryCommandValidator()
    {
        RuleFor(x => x.Request.Id).GreaterThan(0);
        RuleFor(x => x.Request.ParentName).MaximumLength(255).When(x => !string.IsNullOrEmpty(x.Request.ParentName));
        RuleFor(x => x.Request.Mobile).MaximumLength(20).When(x => !string.IsNullOrEmpty(x.Request.Mobile));
        RuleFor(x => x.Request.Email).EmailAddress().When(x => !string.IsNullOrEmpty(x.Request.Email));
        RuleFor(x => x.Request.Source)
            .Must(s => new[] { "website", "walkin", "phone", "referral", "social_media", "other" }.Contains(s))
            .When(x => !string.IsNullOrEmpty(x.Request.Source));
    }
}
