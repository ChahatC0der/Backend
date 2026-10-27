using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Fees.DTOs.Portal;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Fees.Queries.Portal.GetAvailableOptionalFees;

public sealed record GetAvailableOptionalFeesQuery(long StudentId, long AcademicYearId) : IQuery<IReadOnlyList<OptionalFeeResponse>>;
