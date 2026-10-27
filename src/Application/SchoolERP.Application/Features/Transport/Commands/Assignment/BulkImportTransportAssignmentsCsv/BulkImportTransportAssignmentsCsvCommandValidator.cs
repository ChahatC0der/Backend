using FluentValidation;

namespace SchoolERP.Application.Features.Transport.Commands.Assignment.BulkImportTransportAssignmentsCsv;

public sealed class BulkImportTransportAssignmentsCsvCommandValidator : AbstractValidator<BulkImportTransportAssignmentsCsvCommand>
{
    public BulkImportTransportAssignmentsCsvCommandValidator()
    {
        RuleFor(x => x.Request.AcademicYearId).GreaterThan(0);
        RuleFor(x => x.Request.Rows).NotEmpty().Must(r => r.Count <= 500)
            .WithMessage("Bulk import is capped at 500 rows per request in Phase 1.");
        RuleForEach(x => x.Request.Rows).ChildRules(r =>
        {
            r.RuleFor(x => x.StudentId).GreaterThan(0);
            r.RuleFor(x => x.PickupVehicleId).GreaterThan(0);
            r.RuleFor(x => x.PickupRouteId).GreaterThan(0);
            r.RuleFor(x => x.PickupStopId).GreaterThan(0);
        });
    }
}
