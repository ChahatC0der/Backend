using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Domain.Fees.Enums;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Fees.Queries.FeeHead.ExportFeeHeads;

public sealed record ExportFeeHeadsQuery(FeeHeadStatus? Status = null) : IQuery<byte[]>;
