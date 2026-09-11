using FluentValidation;

namespace SchoolERP.Application.Features.Staff.Commands.Staff.PatchStaff;

public class PatchStaffCommandValidator : AbstractValidator<PatchStaffCommand>
{
    public PatchStaffCommandValidator()
    {
        RuleFor(x => x.Request.Id).GreaterThan(0);
        RuleFor(x => x.Request.FirstName).MaximumLength(255).When(x => !string.IsNullOrEmpty(x.Request.FirstName));
        RuleFor(x => x.Request.LastName).MaximumLength(255).When(x => !string.IsNullOrEmpty(x.Request.LastName));
        RuleFor(x => x.Request.Status)
            .Must(s => new[] { "active", "inactive", "suspended", "terminated" }.Contains(s))
            .When(x => !string.IsNullOrEmpty(x.Request.Status));
    }
}
