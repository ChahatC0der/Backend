using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Transport.DTOs.Assignment;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Transport.Queries.Assignment.GetStudentTransportAssignment;

public sealed record GetStudentTransportAssignmentQuery(long StudentId, long? AcademicYearId = null) : IQuery<TransportAssignmentResponse?>;
