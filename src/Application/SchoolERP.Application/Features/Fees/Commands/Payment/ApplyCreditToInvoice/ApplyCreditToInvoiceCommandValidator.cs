using FluentValidation;

namespace SchoolERP.Application.Features.Fees.Commands.Payment.ApplyCreditToInvoice;

public sealed class ApplyCreditToInvoiceCommandValidator : AbstractValidator<ApplyCreditToInvoiceCommand>
{
    public ApplyCreditToInvoiceCommandValidator()
    {
        RuleFor(x => x.Request.InvoiceId).GreaterThan(0);
        RuleFor(x => x.Request.AmountToApply).GreaterThan(0);
    }
}
