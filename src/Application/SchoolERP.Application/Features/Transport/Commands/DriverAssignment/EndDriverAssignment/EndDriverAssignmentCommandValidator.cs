using FluentValidation;

namespace SchoolERP.Application.Features.Transport.Commands.DriverAssignment.EndDriverAssignment;

public sealed class EndDriverAssignmentCommandValidator : AbstractValidator<EndDriverAssignmentCommand>
{
    public EndDriverAssignmentCommandValidator()
    {
        RuleFor(x => x.AssignmentId).GreaterThan(0);
    }
}
