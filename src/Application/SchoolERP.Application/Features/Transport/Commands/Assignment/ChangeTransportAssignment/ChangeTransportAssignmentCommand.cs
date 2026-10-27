using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Transport.DTOs.Assignment;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Transport.Commands.Assignment.ChangeTransportAssignment;

public record ChangeTransportAssignmentCommand(ChangeTransportAssignmentRequest Request) : ICommand<TransportAssignmentResponse>;
