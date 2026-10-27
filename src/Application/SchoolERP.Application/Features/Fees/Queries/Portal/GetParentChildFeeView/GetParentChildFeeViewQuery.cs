using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Fees.DTOs.Portal;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Fees.Queries.Portal.GetParentChildFeeView;

public sealed record GetParentChildFeeViewQuery(long StudentId, long? AcademicYearId = null) : IQuery<ParentChildFeeViewResponse>;
