using FluentValidation;

namespace SchoolERP.Application.Features.Fees.Commands.Invoice.GenerateStudentInvoice;

public sealed class GenerateStudentInvoiceCommandValidator : AbstractValidator<GenerateStudentInvoiceCommand>
{
    public GenerateStudentInvoiceCommandValidator()
    {
        RuleFor(x => x.Request.StudentId).GreaterThan(0);
        RuleFor(x => x.Request.AcademicYearId).GreaterThan(0);
        // Billing period sequence must be a positive number.
        // Maximum sequence will be validated against the tenant's configured frequency later.
        RuleFor(x => x.Request.PeriodSequenceNumbers)
     .NotNull()
     .WithMessage("Period sequence numbers list is required.")
     .NotEmpty()
     .WithMessage("At least one period sequence number is required.");

        RuleForEach(x => x.Request.PeriodSequenceNumbers)
            .GreaterThan(0)
            .WithMessage("Each period sequence number must be greater than zero.");
    }
}