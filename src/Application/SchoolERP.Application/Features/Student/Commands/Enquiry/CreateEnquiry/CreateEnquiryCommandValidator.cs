using FluentValidation;

namespace SchoolERP.Application.Features.Student.Commands.Enquiry.CreateEnquiry;

public class CreateEnquiryCommandValidator : AbstractValidator<CreateEnquiryCommand>
{
    public CreateEnquiryCommandValidator()
    {
        RuleFor(x => x.Request.ParentName).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Request.Mobile).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Request.Email).EmailAddress().When(x => !string.IsNullOrEmpty(x.Request.Email));
        RuleFor(x => x.Request.Source).NotEmpty().MaximumLength(50)
            .Must(s => new[] { "website", "walkin", "phone", "referral", "social_media", "other" }.Contains(s));
    }
}
