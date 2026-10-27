using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Transport.DTOs.DriverAssignment;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Transport.Commands.DriverAssignment.EndDriverAssignment;

public record EndDriverAssignmentCommand(long AssignmentId, EndDriverAssignmentRequest Request) : ICommand<DriverAssignmentResponse>;
