using FluentValidation;

namespace SchoolERP.Application.Features.Student.Commands.Enquiry.ConvertEnquiryToRegistration;

public class ConvertEnquiryToRegistrationCommandValidator : AbstractValidator<ConvertEnquiryToRegistrationCommand>
{
    public ConvertEnquiryToRegistrationCommandValidator()
    {
        RuleFor(x => x.Request.Id).GreaterThan(0);
        RuleFor(x => x.Request.AcademicYearId).GreaterThan(0);
        RuleFor(x => x.Request.ClassId).GreaterThan(0);
    }
}
