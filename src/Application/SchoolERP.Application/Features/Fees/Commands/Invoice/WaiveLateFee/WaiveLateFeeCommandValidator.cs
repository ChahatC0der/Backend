using FluentValidation;

namespace SchoolERP.Application.Features.Fees.Commands.Invoice.WaiveLateFee;

public sealed class WaiveLateFeeCommandValidator : AbstractValidator<WaiveLateFeeCommand>
{
    public WaiveLateFeeCommandValidator()
    {
        RuleFor(x => x.InvoiceId).GreaterThan(0);
        RuleFor(x => x.Request.WaiverReason).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Request.WaiveAmount).GreaterThan(0).When(x => x.Request.WaiveAmount.HasValue);
    }
}
