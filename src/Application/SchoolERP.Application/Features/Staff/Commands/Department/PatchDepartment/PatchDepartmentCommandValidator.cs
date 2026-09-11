using FluentValidation;

namespace SchoolERP.Application.Features.Staff.Commands.Department.PatchDepartment;

public class PatchDepartmentCommandValidator : AbstractValidator<PatchDepartmentCommand>
{
    public PatchDepartmentCommandValidator()
    {
        RuleFor(x => x.Request.Id).GreaterThan(0);
        RuleFor(x => x.Request.Name).MaximumLength(100).When(x => !string.IsNullOrEmpty(x.Request.Name));
        RuleFor(x => x.Request.Code).MaximumLength(20).When(x => !string.IsNullOrEmpty(x.Request.Code));
    }
}
