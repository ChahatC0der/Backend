using FluentValidation;

namespace SchoolERP.Application.Features.Transport.Commands.Vehicle.RestoreVehicle;

public sealed class RestoreVehicleCommandValidator : AbstractValidator<RestoreVehicleCommand>
{
    public RestoreVehicleCommandValidator() => RuleFor(x => x.Id).GreaterThan(0);
}
