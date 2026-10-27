using FluentValidation;

namespace SchoolERP.Application.Features.Transport.Commands.Stop.ReorderStops;

public sealed class ReorderStopsCommandValidator : AbstractValidator<ReorderStopsCommand>
{
    public ReorderStopsCommandValidator()
    {
        RuleFor(x => x.RouteId).GreaterThan(0);
        RuleFor(x => x.Request.StopIds).NotEmpty();
        RuleFor(x => x.Request.StopIds).Must(ids => ids.Distinct().Count() == ids.Count)
            .WithMessage("StopIds must be unique.");
    }
}
