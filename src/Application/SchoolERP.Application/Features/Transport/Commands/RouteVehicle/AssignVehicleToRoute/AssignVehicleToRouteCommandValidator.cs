using FluentValidation;

namespace SchoolERP.Application.Features.Transport.Commands.RouteVehicle.AssignVehicleToRoute;

public sealed class AssignVehicleToRouteCommandValidator : AbstractValidator<AssignVehicleToRouteCommand>
{
    public AssignVehicleToRouteCommandValidator()
    {
        RuleFor(x => x.RouteId).GreaterThan(0);
        RuleFor(x => x.Request.VehicleId).GreaterThan(0);
        RuleFor(x => x.Request.ValidFrom).Must(d => d != default(DateOnly));
    }
}
