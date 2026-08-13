using FluentValidation;

namespace SchoolERP.Application.Features.Tenants.Commands.CreateTenant;

public class CreateTenantValidator : AbstractValidator<CreateTenantCommand>
{
    public CreateTenantValidator()
    {
        RuleFor(x => x.Request.Name)
            .NotEmpty().WithMessage("Tenant name is required.")
            .MaximumLength(255);

        RuleFor(x => x.Request.ContactEmail)
            .NotEmpty().WithMessage("Contact email is required.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(x => x.Request.Plan)
            .Must(p => new[] { "free", "basic", "pro", "enterprise" }.Contains(p))
            .WithMessage("Plan must be one of: free, basic, pro, enterprise.");
    }
}