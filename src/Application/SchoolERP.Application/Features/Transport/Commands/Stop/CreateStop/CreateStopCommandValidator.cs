using FluentValidation;

namespace SchoolERP.Application.Features.Transport.Commands.Stop.CreateStop;

public sealed class CreateStopCommandValidator : AbstractValidator<CreateStopCommand>
{
    public CreateStopCommandValidator()
    {
        RuleFor(x => x.RouteId).GreaterThan(0);
        RuleFor(x => x.Request.Name).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Request.StopOrder).GreaterThan(0);
        RuleFor(x => x.Request.Landmark).MaximumLength(255);
        RuleFor(x => x.Request.StopType)
            .Must(t => t is "pickup_only" or "drop_only" or "both")
            .WithMessage("StopType must be one of: pickup_only, drop_only, both.");
    }
}
