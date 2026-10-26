using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Fees.DTOs.FeeHead;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Fees.Queries.FeeHead.GetFeeHeadById;

public sealed record GetFeeHeadByIdQuery(long Id) : IQuery<FeeHeadResponse>;
