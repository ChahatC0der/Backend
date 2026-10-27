using FluentValidation;

namespace SchoolERP.Application.Features.Transport.Commands.Stop.UpdateStop;

public sealed class UpdateStopCommandValidator : AbstractValidator<UpdateStopCommand>
{
    public UpdateStopCommandValidator()
    {
        RuleFor(x => x.RouteId).GreaterThan(0);
        RuleFor(x => x.StopId).GreaterThan(0);
        RuleFor(x => x.Request.Name).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Request.StopOrder).GreaterThan(0);
        RuleFor(x => x.Request.StopType)
            .Must(t => t is "pickup_only" or "drop_only" or "both");
    }
}
