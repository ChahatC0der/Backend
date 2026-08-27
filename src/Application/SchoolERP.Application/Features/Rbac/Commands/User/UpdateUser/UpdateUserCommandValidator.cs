using FluentValidation;

namespace SchoolERP.Application.Features.Rbac.Commands.User.UpdateUser;

public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.Request.Id).GreaterThan(0).WithMessage("User id is required.");
        RuleFor(x => x.Request.Name).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Request.Phone).MaximumLength(50).When(x => !string.IsNullOrEmpty(x.Request.Phone));
        RuleFor(x => x.Request.Status)
            .Must(status => new[] { "active", "inactive", "suspended" }.Contains(status))
            .When(x => !string.IsNullOrEmpty(x.Request.Status))
            .WithMessage("Invalid status.");
    }
}