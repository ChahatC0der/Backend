using FluentValidation;

namespace SchoolERP.Application.Features.Fees.Commands.Invoice.BulkGenerateInvoices;

public sealed class BulkGenerateInvoicesCommandValidator : AbstractValidator<BulkGenerateInvoicesCommand>
{
    public BulkGenerateInvoicesCommandValidator()
    {
        RuleFor(x => x.Request.AcademicYearId).GreaterThan(0);
        RuleFor(x => x.Request.PeriodLabel).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Request)
            .Must(r => r.ClassGroupId.HasValue || r.ClassId.HasValue)
            .WithMessage("At least one of ClassGroupId or ClassId is required.");
    }
}
