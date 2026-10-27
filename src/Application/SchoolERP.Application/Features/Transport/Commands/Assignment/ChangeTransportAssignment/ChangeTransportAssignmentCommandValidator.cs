using FluentValidation;

namespace SchoolERP.Application.Features.Transport.Commands.Assignment.ChangeTransportAssignment;

public sealed class ChangeTransportAssignmentCommandValidator : AbstractValidator<ChangeTransportAssignmentCommand>
{
    public ChangeTransportAssignmentCommandValidator()
    {
        RuleFor(x => x.Request.StudentId).GreaterThan(0);
        RuleFor(x => x.Request.AcademicYearId).GreaterThan(0);
        RuleFor(x => x.Request.PickupVehicleId).GreaterThan(0);
        RuleFor(x => x.Request.PickupRouteId).GreaterThan(0);
        RuleFor(x => x.Request.PickupStopId).GreaterThan(0);
        RuleFor(x => x.Request.ValidFrom).Must(d => d != default(DateOnly));
    }
}
