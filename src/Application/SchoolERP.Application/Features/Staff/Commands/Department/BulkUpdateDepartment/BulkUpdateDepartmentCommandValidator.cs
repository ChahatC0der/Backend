using FluentValidation;

namespace SchoolERP.Application.Features.Staff.Commands.Department.BulkUpdateDepartment;

public class BulkUpdateDepartmentCommandValidator : AbstractValidator<BulkUpdateDepartmentCommand>
{
    public BulkUpdateDepartmentCommandValidator()
    {
        RuleFor(x => x.Request.Ids).NotEmpty();
        RuleFor(x => x.Request.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Request.Code).NotEmpty().MaximumLength(20);
    }
}
