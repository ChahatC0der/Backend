using FluentValidation;

namespace SchoolERP.Application.Features.Fees.Commands.FeeHead.DeleteFeeHead;

public sealed class DeleteFeeHeadCommandValidator : AbstractValidator<DeleteFeeHeadCommand>
{
    public DeleteFeeHeadCommandValidator() => RuleFor(x => x.Id).GreaterThan(0);
}
