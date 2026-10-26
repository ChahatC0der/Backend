using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.DTOs;
using SchoolERP.Application.Features.Fees.DTOs.Concession;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Fees.Queries.Concession.GetConcessions;

public record class GetConcessionsQuery : PagedRequest, IQuery<PagedResponse<ConcessionResponse>>
{
    public string? Type { get; set; }
    public bool? IsAutoApply { get; set; }
}
