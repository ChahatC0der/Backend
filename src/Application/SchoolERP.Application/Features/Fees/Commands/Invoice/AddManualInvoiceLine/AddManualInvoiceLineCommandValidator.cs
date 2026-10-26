using FluentValidation;

namespace SchoolERP.Application.Features.Fees.Commands.Invoice.AddManualInvoiceLine;

public sealed class AddManualInvoiceLineCommandValidator : AbstractValidator<AddManualInvoiceLineCommand>
{
    public AddManualInvoiceLineCommandValidator()
    {
        RuleFor(x => x.InvoiceId).GreaterThan(0);
        RuleFor(x => x.Request.FeeHeadId).GreaterThan(0);
        RuleFor(x => x.Request.Amount).GreaterThan(0);
    }
}
