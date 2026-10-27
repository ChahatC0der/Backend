using FluentValidation;

namespace SchoolERP.Application.Features.Transport.Commands.Vehicle.PatchVehicleStatus;

public sealed class PatchVehicleStatusCommandValidator : AbstractValidator<PatchVehicleStatusCommand>
{
    public PatchVehicleStatusCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Request.Status)
            .Must(s => s is "active" or "maintenance" or "retired")
            .WithMessage("Status must be one of: active, maintenance, retired.");
    }
}
