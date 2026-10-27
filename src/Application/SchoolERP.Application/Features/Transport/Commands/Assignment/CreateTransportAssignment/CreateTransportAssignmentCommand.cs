using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Transport.DTOs.Assignment;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Transport.Commands.Assignment.CreateTransportAssignment;

public record CreateTransportAssignmentCommand(CreateTransportAssignmentRequest Request) : ICommand<TransportAssignmentResponse>;
