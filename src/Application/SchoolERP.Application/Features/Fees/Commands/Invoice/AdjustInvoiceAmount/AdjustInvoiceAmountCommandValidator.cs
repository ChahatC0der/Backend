using FluentValidation;

namespace SchoolERP.Application.Features.Fees.Commands.Invoice.AdjustInvoiceAmount;

public sealed class AdjustInvoiceAmountCommandValidator : AbstractValidator<AdjustInvoiceAmountCommand>
{
    public AdjustInvoiceAmountCommandValidator()
    {
        RuleFor(x => x.InvoiceId).GreaterThan(0);
        RuleFor(x => x.Request.InvoiceLineId).GreaterThan(0);
        RuleFor(x => x.Request.NewAmount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Request.Reason).NotEmpty().MaximumLength(500);
    }
}
