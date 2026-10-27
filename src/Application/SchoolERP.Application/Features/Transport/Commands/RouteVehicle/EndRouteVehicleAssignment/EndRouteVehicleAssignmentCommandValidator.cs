using FluentValidation;

namespace SchoolERP.Application.Features.Transport.Commands.RouteVehicle.EndRouteVehicleAssignment;

public sealed class EndRouteVehicleAssignmentCommandValidator : AbstractValidator<EndRouteVehicleAssignmentCommand>
{
    public EndRouteVehicleAssignmentCommandValidator()
    {
        RuleFor(x => x.AssignmentId).GreaterThan(0);
    }
}
