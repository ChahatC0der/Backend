using FluentValidation;

namespace SchoolERP.Application.Features.Staff.Commands.Department.BulkPatchDepartment;

public class BulkPatchDepartmentCommandValidator : AbstractValidator<BulkPatchDepartmentCommand>
{
    public BulkPatchDepartmentCommandValidator()
    {
        RuleFor(x => x.Request.Ids).NotEmpty();
        RuleFor(x => x.Request.Name).MaximumLength(100).When(x => !string.IsNullOrEmpty(x.Request.Name));
        RuleFor(x => x.Request.Code).MaximumLength(20).When(x => !string.IsNullOrEmpty(x.Request.Code));
    }
}
