using FluentValidation;

namespace SchoolERP.Application.Features.Student.Commands.Enquiry.ChangeEnquiryStatus;

public class ChangeEnquiryStatusCommandValidator : AbstractValidator<ChangeEnquiryStatusCommand>
{
    public ChangeEnquiryStatusCommandValidator()
    {
        RuleFor(x => x.Request.Id).GreaterThan(0);
        RuleFor(x => x.Request.NewStatus).NotEmpty()
            .Must(s => new[] { "new", "contacted", "followup_scheduled", "converted", "lost", "junk" }.Contains(s));
    }
}
