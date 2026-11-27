using FluentValidation;

namespace SchoolERP.Application.Features.Fees.Commands.Invoice.BulkGenerateInvoices;

public sealed class BulkGenerateInvoicesCommandValidator : AbstractValidator<BulkGenerateInvoicesCommand>
{
    private const int MaxPeriodsPerRequest = 12;

    public BulkGenerateInvoicesCommandValidator()
    {
        RuleFor(x => x.Request.AcademicYearId).GreaterThan(0);

        RuleFor(x => x.Request.PeriodSequenceNumbers)
            .NotEmpty().WithMessage("At least one PeriodSequenceNumber is required.")
            .Must(list => list.Count <= MaxPeriodsPerRequest)
            .WithMessage($"Bulk generation is capped at {MaxPeriodsPerRequest} periods per request in Phase 1.")
            .Must(list => list.All(s => s > 0))
            .WithMessage("PeriodSequenceNumber values must be greater than zero.");

        RuleFor(x => x.Request)
            .Must(r => r.ClassGroupId.HasValue || r.ClassId.HasValue)
            .WithMessage("At least one of ClassGroupId or ClassId is required.");
    }
}