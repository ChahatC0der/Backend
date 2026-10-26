using FluentValidation;

namespace SchoolERP.Application.Features.Fees.Commands.FeeStructure.AssignStudentFeeStructure;

public sealed class AssignStudentFeeStructureCommandValidator : AbstractValidator<AssignStudentFeeStructureCommand>
{
    public AssignStudentFeeStructureCommandValidator()
    {
        RuleFor(x => x.Request.StudentId).GreaterThan(0);
        RuleFor(x => x.Request.FeeStructureId).GreaterThan(0);
        RuleFor(x => x.Request.AcademicYearId).GreaterThan(0);
        RuleFor(x => x.Request.OverrideAmount).GreaterThanOrEqualTo(0).When(x => x.Request.OverrideAmount.HasValue);
    }
}
