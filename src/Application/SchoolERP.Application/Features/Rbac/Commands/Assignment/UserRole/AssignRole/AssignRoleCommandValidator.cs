using FluentValidation;

public class AssignRoleCommandValidator : AbstractValidator<AssignRoleCommand>
{
    public AssignRoleCommandValidator()
    {
        RuleFor(x => x.Request.UserId).GreaterThan(0);
        RuleFor(x => x.Request.RoleId).GreaterThan(0);
        RuleFor(x => x.Request.ScopeType)
            .NotEmpty()
            .Must(scope => new[] { "tenant", "branch", "class", "student", "transport_route" }.Contains(scope));
        RuleFor(x => x.Request.ScopeValue)
            .NotEmpty().When(x => x.Request.ScopeType != "tenant")
            .MaximumLength(50);
        RuleFor(x => x.Request.ValidTo)
            .GreaterThanOrEqualTo(x => x.Request.ValidFrom)
            .When(x => x.Request.ValidTo.HasValue && x.Request.ValidFrom.HasValue);
    }
}