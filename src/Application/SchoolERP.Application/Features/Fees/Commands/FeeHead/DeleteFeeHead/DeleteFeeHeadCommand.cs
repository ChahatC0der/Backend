using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Fees.Commands.FeeHead.DeleteFeeHead;

public record DeleteFeeHeadCommand(long Id) : ICommand<bool>;
