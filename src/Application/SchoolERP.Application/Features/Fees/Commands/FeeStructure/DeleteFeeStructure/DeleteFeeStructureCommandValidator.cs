using FluentValidation;

namespace SchoolERP.Application.Features.Fees.Commands.FeeStructure.DeleteFeeStructure;

public sealed class DeleteFeeStructureCommandValidator : AbstractValidator<DeleteFeeStructureCommand>
{
    public DeleteFeeStructureCommandValidator() => RuleFor(x => x.Id).GreaterThan(0);
}
