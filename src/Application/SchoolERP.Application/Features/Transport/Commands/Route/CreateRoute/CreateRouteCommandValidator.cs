using FluentValidation;

namespace SchoolERP.Application.Features.Transport.Commands.Route.CreateRoute;

public sealed class CreateRouteCommandValidator : AbstractValidator<CreateRouteCommand>
{
    public CreateRouteCommandValidator()
    {
        RuleFor(x => x.Request.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Request.Code).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Request.Description).MaximumLength(255);
    }
}
