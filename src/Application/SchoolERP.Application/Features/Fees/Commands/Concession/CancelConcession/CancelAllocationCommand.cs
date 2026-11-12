using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Fees.Commands.Concession.CancelAllocation;

public sealed record CancelAllocationCommand(long Id) : ICommand<string>;