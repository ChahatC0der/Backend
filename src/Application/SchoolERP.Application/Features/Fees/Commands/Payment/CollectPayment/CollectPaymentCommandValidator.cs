using FluentValidation;

namespace SchoolERP.Application.Features.Fees.Commands.Payment.CollectPayment;

public sealed class CollectPaymentCommandValidator : AbstractValidator<CollectPaymentCommand>
{
    public CollectPaymentCommandValidator()
    {
        RuleFor(x => x.Request.StudentId).GreaterThan(0);
        RuleFor(x => x.Request.AcademicYearId).GreaterThan(0);
        RuleFor(x => x.Request.TotalAmountReceived).GreaterThan(0);
        RuleFor(x => x.Request.Mode).NotEmpty();
        RuleForEach(x => x.Request.Allocations).ChildRules(a =>
        {
            a.RuleFor(x => x.InvoiceId).GreaterThan(0);
            a.RuleFor(x => x.AmountToAllocate).GreaterThan(0);
        });
    }
}
