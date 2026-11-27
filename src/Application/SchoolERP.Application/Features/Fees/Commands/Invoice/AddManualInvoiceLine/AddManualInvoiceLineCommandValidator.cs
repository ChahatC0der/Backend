using FluentValidation;

namespace SchoolERP.Application.Features.Fees.Commands.Invoice.AddManualInvoiceLine;

public sealed class AddManualInvoiceLineCommandValidator : AbstractValidator<AddManualInvoiceLineCommand>
{
    public AddManualInvoiceLineCommandValidator()
    {
        RuleFor(x => x.InvoiceId).GreaterThan(0);
        RuleFor(x => x.Request).NotNull();
        RuleFor(x => x.Request.FeeHeadId).GreaterThan(0).When(x => x.Request is not null);
        RuleFor(x => x.Request.Amount)
            .GreaterThan(0)
            .LessThanOrEqualTo(10_000_000m)
            .PrecisionScale(18, 2, false)
            .When(x => x.Request is not null);
    }
}
