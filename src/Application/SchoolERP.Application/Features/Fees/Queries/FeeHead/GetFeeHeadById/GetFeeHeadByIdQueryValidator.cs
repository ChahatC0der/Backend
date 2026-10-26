using FluentValidation;

namespace SchoolERP.Application.Features.Fees.Queries.FeeHead.GetFeeHeadById;

public sealed class GetFeeHeadByIdQueryValidator : AbstractValidator<GetFeeHeadByIdQuery>
{
    public GetFeeHeadByIdQueryValidator() => RuleFor(x => x.Id).GreaterThan(0);
}
