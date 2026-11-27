using FluentValidation;

namespace SchoolERP.Application.Features.Fees.Queries.Invoice.GetInvoices;

public sealed class GetInvoicesQueryValidator : AbstractValidator<GetInvoicesQuery>
{
    public GetInvoicesQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.Size).InclusiveBetween(1, 200);
        RuleFor(x => x.AcademicYearId).GreaterThan(0).When(x => x.AcademicYearId.HasValue);
        RuleFor(x => x.StudentId).GreaterThan(0).When(x => x.StudentId.HasValue);
        RuleFor(x => x.ClassId).GreaterThan(0).When(x => x.ClassId.HasValue);
        RuleFor(x => x.SectionId).GreaterThan(0).When(x => x.SectionId.HasValue);
        RuleFor(x => x.PeriodLabel).MaximumLength(50).When(x => !string.IsNullOrWhiteSpace(x.PeriodLabel));
        RuleFor(x => x.PeriodSequenceNumber).GreaterThan(0).When(x => x.PeriodSequenceNumber.HasValue);
        RuleFor(x => x)
            .Must(x => !x.DueDateFrom.HasValue || !x.DueDateTo.HasValue || x.DueDateFrom <= x.DueDateTo)
            .WithMessage("DueDateFrom must be earlier than or equal to DueDateTo.")
            .OverridePropertyName("DueDateFrom");
    }
}
