using FluentValidation;

namespace SchoolERP.Application.Features.Fees.Commands.Invoice.RecalculateLateFees;

public sealed class RecalculateLateFeesCommandValidator : AbstractValidator<RecalculateLateFeesCommand>
{
    public RecalculateLateFeesCommandValidator()
    {
        RuleFor(x => x.Request.InvoiceId).GreaterThan(0).When(x => x.Request.InvoiceId.HasValue);
        RuleFor(x => x.Request.AcademicYearId).GreaterThan(0).When(x => x.Request.AcademicYearId.HasValue);
    }
}
