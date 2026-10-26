using FluentValidation;

namespace SchoolERP.Application.Features.Fees.Commands.FeeStructure.UpdateFeeStructure;

public sealed class UpdateFeeStructureCommandValidator : AbstractValidator<UpdateFeeStructureCommand>
{
    public UpdateFeeStructureCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Request.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Request.ApplicableFrom).LessThanOrEqualTo(x => x.Request.ApplicableTo);
        RuleFor(x => x.Request.Lines).NotEmpty();
        RuleForEach(x => x.Request.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.FeeHeadId).GreaterThan(0);
            line.RuleFor(l => l.Amount).GreaterThanOrEqualTo(0);
        });
    }
}
