using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Fees.DTOs.FeeHead;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Fees.Commands.FeeHead.UpdateFeeHead;

public record UpdateFeeHeadCommand(long Id, UpdateFeeHeadRequest Request) : ICommand<FeeHeadResponse>;
