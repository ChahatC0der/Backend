using FluentValidation;

namespace SchoolERP.Application.Features.Transport.Commands.TransportFee.UpdateTransportFee;

public sealed class UpdateTransportFeeCommandValidator : AbstractValidator<UpdateTransportFeeCommand>
{
    public UpdateTransportFeeCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Request.FeeAmount).GreaterThan(0);
        RuleFor(x => x.Request.Frequency)
            .Must(f => f is "monthly" or "quarterly" or "half_yearly");
    }
}
