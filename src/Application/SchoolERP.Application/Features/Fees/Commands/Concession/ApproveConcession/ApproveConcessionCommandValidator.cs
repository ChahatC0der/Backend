using FluentValidation;

namespace SchoolERP.Application.Features.Fees.Commands.Concession.ApproveConcession;

public sealed class ApproveConcessionCommandValidator : AbstractValidator<ApproveConcessionCommand>
{
    public ApproveConcessionCommandValidator() => RuleFor(x => x.AllocationId).GreaterThan(0);
}
