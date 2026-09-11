using FluentValidation;

namespace SchoolERP.Application.Features.Staff.Commands.Department.CreateDepartment;

public class CreateDepartmentCommandValidator : AbstractValidator<CreateDepartmentCommand>
{
    public CreateDepartmentCommandValidator()
    {
        RuleFor(x => x.Request.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Request.Code).NotEmpty().MaximumLength(20);
    }
}
