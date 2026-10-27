using FluentValidation;

namespace SchoolERP.Application.Features.Transport.Commands.Route.UpdateRoute;

public sealed class UpdateRouteCommandValidator : AbstractValidator<UpdateRouteCommand>
{
    public UpdateRouteCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Request.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Request.Code).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Request.Description).MaximumLength(255);
    }
}
