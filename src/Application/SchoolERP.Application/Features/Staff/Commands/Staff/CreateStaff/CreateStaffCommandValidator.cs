using FluentValidation;

namespace SchoolERP.Application.Features.Staff.Commands.Staff.CreateStaff;

public class CreateStaffCommandValidator : AbstractValidator<CreateStaffCommand>
{
    public CreateStaffCommandValidator()
    {
        RuleFor(x => x.Request.FirstName).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Request.LastName).MaximumLength(255);
        RuleFor(x => x.Request.JoiningDate).NotEmpty();
        RuleFor(x => x.Request.EmploymentType).NotEmpty()
            .Must(t => new[] { "permanent", "contract", "probation", "intern" }.Contains(t));
        RuleFor(x => x.Request.StaffType).NotEmpty()
            .Must(t => new[] { "teaching", "non_teaching" }.Contains(t));
        RuleFor(x => x.Request.Designation).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Request.PersonalEmail).EmailAddress().When(x => !string.IsNullOrEmpty(x.Request.PersonalEmail));
        RuleFor(x => x.Request.WorkEmail).EmailAddress().When(x => !string.IsNullOrEmpty(x.Request.WorkEmail));
        RuleFor(x => x.Request.PersonalMobile).MaximumLength(20).When(x => !string.IsNullOrEmpty(x.Request.PersonalMobile));
        RuleFor(x => x.Request.WorkMobile).MaximumLength(20).When(x => !string.IsNullOrEmpty(x.Request.WorkMobile));
    }
}
