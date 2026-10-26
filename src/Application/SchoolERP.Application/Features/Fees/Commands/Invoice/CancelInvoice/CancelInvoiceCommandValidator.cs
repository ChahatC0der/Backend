using FluentValidation;

namespace SchoolERP.Application.Features.Fees.Commands.Invoice.CancelInvoice;

public sealed class CancelInvoiceCommandValidator : AbstractValidator<CancelInvoiceCommand>
{
    public CancelInvoiceCommandValidator() => RuleFor(x => x.InvoiceId).GreaterThan(0);
}
