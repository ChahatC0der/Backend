using FluentValidation;

namespace SchoolERP.Application.Features.Transport.Commands.VehicleMaintenance.AddVehicleMaintenance;

public sealed class AddVehicleMaintenanceCommandValidator : AbstractValidator<AddVehicleMaintenanceCommand>
{
    public AddVehicleMaintenanceCommandValidator()
    {
        RuleFor(x => x.VehicleId).GreaterThan(0);
        RuleFor(x => x.Request.MaintenanceDate)
            .Must(d => d != default(DateOnly))
            .WithMessage("MaintenanceDate is required.");
        RuleFor(x => x.Request.Cost).GreaterThanOrEqualTo(0).When(x => x.Request.Cost.HasValue);
        RuleFor(x => x.Request.OdometerReading).GreaterThanOrEqualTo(0).When(x => x.Request.OdometerReading.HasValue);
    }
}
