using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Transport.DTOs.Portal;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Transport.Queries.Portal.GetParentChildTransportView;

public sealed record GetParentChildTransportViewQuery(long StudentId, long? AcademicYearId = null) : IQuery<ParentTransportResponse?>;
