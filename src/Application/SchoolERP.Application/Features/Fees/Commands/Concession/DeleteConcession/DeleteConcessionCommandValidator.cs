using FluentValidation;

namespace SchoolERP.Application.Features.Fees.Commands.Concession.DeleteConcession;

public sealed class DeleteConcessionCommandValidator : AbstractValidator<DeleteConcessionCommand>
{
    public DeleteConcessionCommandValidator() => RuleFor(x => x.Id).GreaterThan(0);
}
