using FluentValidation;

namespace SchoolERP.Application.Features.Transport.Commands.Vehicle.CreateVehicle;

public sealed class CreateVehicleCommandValidator : AbstractValidator<CreateVehicleCommand>
{
    public CreateVehicleCommandValidator()
    {
        RuleFor(x => x.Request.VehicleNumber).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Request.VehicleType)
            .Must(t => t is "bus" or "van" or "minibus" or "car" or "other")
            .WithMessage("VehicleType must be one of: bus, van, minibus, car, other.");
        RuleFor(x => x.Request.Capacity).GreaterThan(0);
        RuleFor(x => x.Request.Model).MaximumLength(100);
        RuleFor(x => x.Request.Make).MaximumLength(100);
        RuleFor(x => x.Request.Year).InclusiveBetween(1900, 2100).When(x => x.Request.Year.HasValue);
    }
}
