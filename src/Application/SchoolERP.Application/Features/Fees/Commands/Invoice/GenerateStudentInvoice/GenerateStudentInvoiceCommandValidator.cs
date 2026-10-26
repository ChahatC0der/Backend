using FluentValidation;

namespace SchoolERP.Application.Features.Fees.Commands.Invoice.GenerateStudentInvoice;

public sealed class GenerateStudentInvoiceCommandValidator : AbstractValidator<GenerateStudentInvoiceCommand>
{
    public GenerateStudentInvoiceCommandValidator()
    {
        RuleFor(x => x.Request.StudentId).GreaterThan(0);
        RuleFor(x => x.Request.AcademicYearId).GreaterThan(0);
        RuleFor(x => x.Request.PeriodLabel).NotEmpty().MaximumLength(100);
    }
}
