using FluentValidation;

namespace SchoolERP.Application.Features.Transport.Commands.TransportFee.CreateTransportFee;

public sealed class CreateTransportFeeCommandValidator : AbstractValidator<CreateTransportFeeCommand>
{
    public CreateTransportFeeCommandValidator()
    {
        RuleFor(x => x.Request.AcademicYearId).GreaterThan(0);
        RuleFor(x => x.Request.FeeAmount).GreaterThan(0);
        RuleFor(x => x.Request.Frequency)
            .Must(f => f is "monthly" or "quarterly" or "half_yearly")
            .WithMessage("Frequency must be one of: monthly, quarterly, half_yearly.");
        RuleFor(x => x.Request).Must(r =>
            (r.RouteId.HasValue && !r.StopId.HasValue) || (!r.RouteId.HasValue && r.StopId.HasValue))
            .WithMessage("Exactly one of RouteId or StopId must be provided (XOR).");
    }
}
