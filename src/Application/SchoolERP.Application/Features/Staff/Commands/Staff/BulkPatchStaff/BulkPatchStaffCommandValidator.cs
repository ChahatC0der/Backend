using FluentValidation;

namespace SchoolERP.Application.Features.Staff.Commands.Staff.BulkPatchStaff;

public class BulkPatchStaffCommandValidator : AbstractValidator<BulkPatchStaffCommand>
{
    public BulkPatchStaffCommandValidator()
    {
        RuleFor(x => x.Request.Ids).NotEmpty();
        RuleFor(x => x.Request.EmploymentType)
            .Must(t => new[] { "permanent", "contract", "probation", "intern" }.Contains(t))
            .When(x => !string.IsNullOrEmpty(x.Request.EmploymentType));
        RuleFor(x => x.Request.StaffType)
            .Must(t => new[] { "teaching", "non_teaching" }.Contains(t))
            .When(x => !string.IsNullOrEmpty(x.Request.StaffType));
        RuleFor(x => x.Request.Status)
            .Must(s => new[] { "active", "inactive", "suspended", "terminated" }.Contains(s))
            .When(x => !string.IsNullOrEmpty(x.Request.Status));
    }
}
