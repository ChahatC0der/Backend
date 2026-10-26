using FluentValidation;

namespace SchoolERP.Application.Features.Fees.Commands.Payment.BulkRecordPaymentsCsv;

public sealed class BulkRecordPaymentsCsvCommandValidator : AbstractValidator<BulkRecordPaymentsCsvCommand>
{
    public BulkRecordPaymentsCsvCommandValidator()
    {
        RuleFor(x => x.AcademicYearId).GreaterThan(0);
        RuleFor(x => x.Rows).NotEmpty().Must(r => r.Count <= 500)
            .WithMessage("Bulk payment upload is capped at 500 rows per request in Phase 1.");
        RuleForEach(x => x.Rows).ChildRules(r =>
        {
            r.RuleFor(x => x.StudentId).GreaterThan(0);
            r.RuleFor(x => x.Amount).GreaterThan(0);
            r.RuleFor(x => x.Mode).NotEmpty();
        });
    }
}
