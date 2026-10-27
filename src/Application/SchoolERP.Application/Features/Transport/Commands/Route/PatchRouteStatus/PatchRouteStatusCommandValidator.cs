using FluentValidation;

namespace SchoolERP.Application.Features.Transport.Commands.Route.PatchRouteStatus;

public sealed class PatchRouteStatusCommandValidator : AbstractValidator<PatchRouteStatusCommand>
{
    public PatchRouteStatusCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Request.Status)
            .Must(s => s is "active" or "inactive")
            .WithMessage("Status must be one of: active, inactive.");
    }
}
