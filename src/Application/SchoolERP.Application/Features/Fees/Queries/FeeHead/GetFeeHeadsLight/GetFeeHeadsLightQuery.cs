using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Fees.DTOs.FeeHead;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Fees.Queries.FeeHead.GetFeeHeadsLight;

public sealed record GetFeeHeadsLightQuery : IQuery<IReadOnlyList<FeeHeadLightResponse>>;
