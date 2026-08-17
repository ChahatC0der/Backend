using FluentValidation;

namespace SchoolERP.Application.Features.Tenants.Commands.UpdateTenant;

public class UpdateTenantValidator : AbstractValidator<UpdateTenantCommand>
{
    public UpdateTenantValidator()
    {
        RuleFor(x => x.Request.Code)
            .NotEmpty().MaximumLength(20)
            .Matches("^[A-Z0-9]+$").WithMessage("Code must be uppercase alphanumeric.");

        RuleFor(x => x.Request.Name)
            .NotEmpty().MaximumLength(255);

        RuleFor(x => x.Request.Subdomain)
            .NotEmpty().MaximumLength(100)
            .Matches("^[a-z0-9-]+$").WithMessage("Subdomain can only contain lowercase letters, numbers, and hyphens.");

        RuleFor(x => x.Request.ContactEmail)
            .NotEmpty().EmailAddress();

        RuleFor(x => x.Request.Plan)
            .Must(p => new[] { "free", "basic", "pro", "enterprise" }.Contains(p));

        RuleFor(x => x.Request.Status)
            .Must(s => new[] { "active", "suspended", "trial", "expired" }.Contains(s));
    }
}