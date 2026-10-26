using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Fees.DTOs.FeeHead;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Fees.Commands.FeeHead.PatchFeeHead;

public record PatchFeeHeadCommand(long Id, PatchFeeHeadRequest Request) : ICommand<FeeHeadResponse>;
