using FluentValidation;

namespace SchoolERP.Application.Features.Transport.Commands.Assignment.EndTransportAssignment;

public sealed class EndTransportAssignmentCommandValidator : AbstractValidator<EndTransportAssignmentCommand>
{
    public EndTransportAssignmentCommandValidator()
    {
        RuleFor(x => x.AssignmentId).GreaterThan(0);
    }
}
