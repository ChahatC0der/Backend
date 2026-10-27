using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Transport.DTOs.Assignment;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Transport.Queries.Assignment.GetStudentTransportFeeQuote;

public sealed record GetStudentTransportFeeQuoteQuery(long StudentId, long AcademicYearId) : IQuery<TransportAssignmentFeeQuoteResponse?>;
