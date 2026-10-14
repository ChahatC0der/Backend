using FluentValidation;

namespace SchoolERP.Application.Features.Student.Commands.Enquiry.AssignEnquiry;

public class AssignEnquiryCommandValidator : AbstractValidator<AssignEnquiryCommand>
{
    public AssignEnquiryCommandValidator()
    {
        RuleFor(x => x.Request.Id).GreaterThan(0);
        RuleFor(x => x.Request.AssignedTo).GreaterThan(0);
    }
}
