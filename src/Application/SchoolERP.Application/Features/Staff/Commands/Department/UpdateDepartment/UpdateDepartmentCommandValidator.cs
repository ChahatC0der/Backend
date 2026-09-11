using FluentValidation;

namespace SchoolERP.Application.Features.Staff.Commands.Department.UpdateDepartment;

public class UpdateDepartmentCommandValidator : AbstractValidator<UpdateDepartmentCommand>
{
    public UpdateDepartmentCommandValidator()
    {
        RuleFor(x => x.Request.Id).GreaterThan(0);
        RuleFor(x => x.Request.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Request.Code).NotEmpty().MaximumLength(20);
    }
}
