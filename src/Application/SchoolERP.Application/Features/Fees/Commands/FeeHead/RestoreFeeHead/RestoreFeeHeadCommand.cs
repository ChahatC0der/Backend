using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Fees.Commands.FeeHead.RestoreFeeHead;

public record RestoreFeeHeadCommand(long Id) : ICommand<bool>;
