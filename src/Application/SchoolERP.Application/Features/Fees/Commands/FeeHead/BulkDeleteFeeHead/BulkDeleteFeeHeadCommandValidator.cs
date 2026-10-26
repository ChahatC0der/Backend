using FluentValidation;

namespace SchoolERP.Application.Features.Fees.Commands.FeeHead.BulkDeleteFeeHead;

public sealed class BulkDeleteFeeHeadCommandValidator : AbstractValidator<BulkDeleteFeeHeadCommand>
{
    public BulkDeleteFeeHeadCommandValidator()
    {
        RuleFor(x => x.Ids).NotEmpty().Must(ids => ids.Count <= 500)
            .WithMessage("Max 500 IDs per bulk delete.");
        RuleForEach(x => x.Ids).GreaterThan(0);
    }
}
