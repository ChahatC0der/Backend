using FluentValidation;

namespace SchoolERP.Application.Features.Transport.Commands.Stop.DeleteStop;

public sealed class DeleteStopCommandValidator : AbstractValidator<DeleteStopCommand>
{
    public DeleteStopCommandValidator()
    {
        RuleFor(x => x.RouteId).GreaterThan(0);
        RuleFor(x => x.StopId).GreaterThan(0);
    }
}
