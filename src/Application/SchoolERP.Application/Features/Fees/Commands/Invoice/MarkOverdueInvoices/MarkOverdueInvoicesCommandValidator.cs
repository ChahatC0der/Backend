using FluentValidation;

namespace SchoolERP.Application.Features.Fees.Commands.Invoice.MarkOverdueInvoices;

public sealed class MarkOverdueInvoicesCommandValidator : AbstractValidator<MarkOverdueInvoicesCommand>
{
    public MarkOverdueInvoicesCommandValidator()
    {
        RuleFor(x => x.AcademicYearId).GreaterThan(0).When(x => x.AcademicYearId.HasValue);
    }
}
