using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Transport.DTOs.Assignment;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Transport.Commands.Assignment.EndTransportAssignment;

public record EndTransportAssignmentCommand(long AssignmentId, EndTransportAssignmentRequest Request) : ICommand<TransportAssignmentResponse>;
