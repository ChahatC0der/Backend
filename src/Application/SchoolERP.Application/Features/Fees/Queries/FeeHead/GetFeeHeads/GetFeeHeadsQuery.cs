using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.DTOs;
using SchoolERP.Application.Features.Fees.DTOs.FeeHead;
using SchoolERP.Domain.Fees.Enums;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Fees.Queries.FeeHead.GetFeeHeads;

public sealed record GetFeeHeadsQuery : PagedRequest, IQuery<PagedResponse<FeeHeadResponse>>
{
    public FeeHeadStatus? Status { get; set; }
    public bool? IsOptional { get; set; }
    public bool? IsMandatory { get; set; }
}
