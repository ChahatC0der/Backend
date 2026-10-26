using FluentValidation;

namespace SchoolERP.Application.Features.Fees.Commands.FeeStructure.CloneFeeStructure;

public sealed class CloneFeeStructureCommandValidator : AbstractValidator<CloneFeeStructureCommand>
{
    public CloneFeeStructureCommandValidator()
    {
        RuleFor(x => x.SourceId).GreaterThan(0);
        RuleFor(x => x.Request.TargetAcademicYearId).GreaterThan(0);
        RuleFor(x => x.Request.PercentageAdjustment)
            .GreaterThanOrEqualTo(-100).When(x => x.Request.PercentageAdjustment.HasValue);
        RuleFor(x => x.Request).Must(r =>
            !(r.PercentageAdjustment.HasValue && r.FlatAdjustment.HasValue))
            .WithMessage("Provide either PercentageAdjustment OR FlatAdjustment, not both.");
    }
}
