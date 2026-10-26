using FluentValidation;

namespace SchoolERP.Application.Features.Fees.Commands.Invoice.RegenerateInvoice;

public sealed class RegenerateInvoiceCommandValidator : AbstractValidator<RegenerateInvoiceCommand>
{
    public RegenerateInvoiceCommandValidator() => RuleFor(x => x.InvoiceId).GreaterThan(0);
}
