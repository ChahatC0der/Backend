using FluentValidation;

namespace SchoolERP.Application.Features.Staff.Commands.Staff.BulkUpdateStaff;

public class BulkUpdateStaffCommandValidator : AbstractValidator<BulkUpdateStaffCommand>
{
    public BulkUpdateStaffCommandValidator()
    {
        RuleFor(x => x.Request.Ids).NotEmpty();
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
