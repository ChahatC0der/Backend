using FluentValidation;

namespace SchoolERP.Application.Features.Fees.Commands.FeeStructure.CreateFeeStructure;

public sealed class CreateFeeStructureCommandValidator : AbstractValidator<CreateFeeStructureCommand>
{
    public CreateFeeStructureCommandValidator()
    {
        RuleFor(x => x.Request.AcademicYearId).GreaterThan(0);
        RuleFor(x => x.Request.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Request.ApplicableFrom).LessThanOrEqualTo(x => x.Request.ApplicableTo)
            .WithMessage("ApplicableFrom must be before ApplicableTo.");

        RuleFor(x => x.Request)
            .Must(r => r.ClassId.HasValue || r.ClassGroupId.HasValue)
            .WithMessage("At least one of ClassId or ClassGroupId must be provided.");

        RuleFor(x => x.Request.Lines).NotEmpty().WithMessage("At least one fee line is required.");
        RuleForEach(x => x.Request.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.FeeHeadId).GreaterThan(0);
            line.RuleFor(l => l.Amount).GreaterThanOrEqualTo(0);
        });
    }
}
