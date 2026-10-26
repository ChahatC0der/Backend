using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.DTOs;
using SchoolERP.Application.Features.Fees.DTOs.Refund;
using SchoolERP.Domain.Fees.Enums;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Fees.Queries.Refund.GetRefunds;

public sealed record GetRefundsQuery : PagedRequest, IQuery<PagedResponse<RefundResponse>>
{
    public long? StudentId { get; set; }
    public RefundStatus? Status { get; set; }
}
