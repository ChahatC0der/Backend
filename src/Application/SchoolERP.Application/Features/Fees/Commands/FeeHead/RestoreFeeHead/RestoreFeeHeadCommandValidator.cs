using FluentValidation;

namespace SchoolERP.Application.Features.Fees.Commands.FeeHead.RestoreFeeHead;

public sealed class RestoreFeeHeadCommandValidator : AbstractValidator<RestoreFeeHeadCommand>
{
    public RestoreFeeHeadCommandValidator() => RuleFor(x => x.Id).GreaterThan(0);
}
