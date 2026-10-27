using FluentValidation;

namespace SchoolERP.Application.Features.Transport.Commands.Vehicle.UpdateVehicle;

public sealed class UpdateVehicleCommandValidator : AbstractValidator<UpdateVehicleCommand>
{
    public UpdateVehicleCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Request.VehicleNumber).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Request.VehicleType)
            .Must(t => t is "bus" or "van" or "minibus" or "car" or "other");
        RuleFor(x => x.Request.Capacity).GreaterThan(0);
        RuleFor(x => x.Request.Year).InclusiveBetween(1900, 2100).When(x => x.Request.Year.HasValue);
    }
}
