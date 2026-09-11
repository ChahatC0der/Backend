using FluentValidation;

namespace SchoolERP.Application.Features.Staff.Commands.Staff.UpdateStaff;

public class UpdateStaffCommandValidator : AbstractValidator<UpdateStaffCommand>
{
    public UpdateStaffCommandValidator()
    {
        RuleFor(x => x.Request.Id).GreaterThan(0);
        RuleFor(x => x.Request.FirstName).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Request.LastName).MaximumLength(255);
        RuleFor(x => x.Request.JoiningDate).NotEmpty();
        RuleFor(x => x.Request.EmploymentType).NotEmpty()
            .Must(t => new[] { "permanent", "contract", "probation", "intern" }.Contains(t));
        RuleFor(x => x.Request.StaffType).NotEmpty()
            .Must(t => new[] { "teaching", "non_teaching" }.Contains(t));
        RuleFor(x => x.Request.Designation).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Request.Status)
            .Must(s => new[] { "active", "inactive", "suspended", "terminated" }.Contains(s))
            .When(x => !string.IsNullOrEmpty(x.Request.Status));
    }
}
