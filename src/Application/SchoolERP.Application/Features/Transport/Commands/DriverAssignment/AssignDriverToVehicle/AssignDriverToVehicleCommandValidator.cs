using FluentValidation;

namespace SchoolERP.Application.Features.Transport.Commands.DriverAssignment.AssignDriverToVehicle;

public sealed class AssignDriverToVehicleCommandValidator : AbstractValidator<AssignDriverToVehicleCommand>
{
    public AssignDriverToVehicleCommandValidator()
    {
        RuleFor(x => x.VehicleId).GreaterThan(0);
        RuleFor(x => x.Request.DriverStaffId).GreaterThan(0);
        RuleFor(x => x.Request.ValidFrom).Must(d => d != default(DateOnly));
    }
}
