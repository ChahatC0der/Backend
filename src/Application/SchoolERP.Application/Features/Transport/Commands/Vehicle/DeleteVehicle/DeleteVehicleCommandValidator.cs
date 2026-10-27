using FluentValidation;

namespace SchoolERP.Application.Features.Transport.Commands.Vehicle.DeleteVehicle;

public sealed class DeleteVehicleCommandValidator : AbstractValidator<DeleteVehicleCommand>
{
    public DeleteVehicleCommandValidator() => RuleFor(x => x.Id).GreaterThan(0);
}
