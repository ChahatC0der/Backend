using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Fees.Commands.FeeHead.BulkDeleteFeeHead;

/// <summary>Max 500 IDs per request (Phase 1 cap — see docs/fees-phase1-scope.md).</summary>
public record BulkDeleteFeeHeadCommand(IReadOnlyList<long> Ids) : ICommand<int>;
