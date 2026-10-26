using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.DTOs;
using SchoolERP.Application.Features.Fees.DTOs.Concession;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Fees.Queries.Concession.GetPendingConcessions;

public sealed record GetPendingConcessionsQuery : PagedRequest, IQuery<PagedResponse<ConcessionAllocationResponse>>;
