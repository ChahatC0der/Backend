using FluentValidation;

namespace SchoolERP.Application.Features.Fees.Commands.FeeStructure.PatchFeeStructureStatus;

public sealed class PatchFeeStructureStatusCommandValidator : AbstractValidator<PatchFeeStructureStatusCommand>
{
    public PatchFeeStructureStatusCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Request.Status).NotEmpty()
            .Must(s => s is "draft" or "active" or "archived")
            .WithMessage("Status must be one of: draft, active, archived.");
    }
}
