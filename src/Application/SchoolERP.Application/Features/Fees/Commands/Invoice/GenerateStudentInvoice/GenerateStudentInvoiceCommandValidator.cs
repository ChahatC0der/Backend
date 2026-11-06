using FluentValidation;

namespace SchoolERP.Application.Features.Fees.Commands.Invoice.GenerateStudentInvoice;

public sealed class GenerateStudentInvoiceCommandValidator : AbstractValidator<GenerateStudentInvoiceCommand>
{
    public GenerateStudentInvoiceCommandValidator()
    {
        RuleFor(x => x.Request.StudentId).GreaterThan(0);
        RuleFor(x => x.Request.AcademicYearId).GreaterThan(0);
        RuleFor(x => x.Request.PeriodLabel).NotEmpty().MaximumLength(100);
        // Billing period sequence must be a positive number.
        // Maximum sequence will be validated against the tenant's configured frequency later.
        RuleFor(x => x.Request.PeriodSequenceNumber)
            .GreaterThan(0)
            .WithMessage("Period sequence number must be greater than zero.");
    }
}
