using FluentValidation;

namespace SchoolERP.Application.Features.Fees.Commands.Concession.AutoApplySiblingConcessions;

public sealed class AutoApplySiblingConcessionsCommandValidator : AbstractValidator<AutoApplySiblingConcessionsCommand>
{
    public AutoApplySiblingConcessionsCommandValidator() => RuleFor(x => x.AcademicYearId).GreaterThan(0);
}
