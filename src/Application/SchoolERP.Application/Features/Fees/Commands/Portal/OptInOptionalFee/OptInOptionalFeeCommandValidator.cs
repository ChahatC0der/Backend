using FluentValidation;

namespace SchoolERP.Application.Features.Fees.Commands.Portal.OptInOptionalFee;

public sealed class OptInOptionalFeeCommandValidator : AbstractValidator<OptInOptionalFeeCommand>
{
    public OptInOptionalFeeCommandValidator()
    {
        RuleFor(x => x.Request.StudentId).GreaterThan(0);
        RuleFor(x => x.Request.AcademicYearId).GreaterThan(0);
        RuleFor(x => x.Request.FeeHeadId).GreaterThan(0);
        RuleFor(x => x.Request.Amount).GreaterThan(0);
    }
}
