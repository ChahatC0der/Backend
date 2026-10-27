using FluentValidation;

namespace SchoolERP.Application.Features.Transport.Commands.Route.DeleteRoute;

public sealed class DeleteRouteCommandValidator : AbstractValidator<DeleteRouteCommand>
{
    public DeleteRouteCommandValidator() => RuleFor(x => x.Id).GreaterThan(0);
}
