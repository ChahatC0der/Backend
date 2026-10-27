using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Fees.Builders;
using SchoolERP.Application.Features.Transport.DTOs.Stop;
using SchoolERP.Domain.Shared.Results;
using RouteEntity = SchoolERP.Domain.Transport.Entities.Route;
using StopEntity = SchoolERP.Domain.Transport.Entities.Stop;

namespace SchoolERP.Application.Features.Transport.Commands.Stop.ReorderStops;

/// <summary>
/// Atomic reorder. Two-phase to avoid UQ_Stops_Route_Order collision mid-update:
///   Phase 1: assign all stops temporary negative StopOrder (unique, no clash).
///   Phase 2: assign final StopOrder = index + 1 in requested order.
/// Both SaveChangesAsync calls run inside the pipeline transaction.
/// </summary>
public sealed class ReorderStopsCommandHandler : IRequestHandler<ReorderStopsCommand, Result<IReadOnlyList<StopResponse>>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;

    public ReorderStopsCommandHandler(IApplicationDbContext db, ICurrentTenantService tenant, ICurrentBranchService branch)
    { _db = db; _tenant = tenant; _branch = branch; }

    public async Task<Result<IReadOnlyList<StopResponse>>> Handle(ReorderStopsCommand request, CancellationToken ct)
    {
        var scope = TransportScopeGuard.RequireBranch(_tenant, _branch);
        if (scope.IsFailure) return scope.Error!;
        var (_, branchId) = scope.Value;

        var routeOk = await _db.Set<RouteEntity>().AnyAsync(r =>
            r.Id == request.RouteId && r.BranchId == branchId && !r.IsDeleted, ct);
        if (!routeOk) return Error.NotFound("Route", request.RouteId.ToString());

        var stops = await _db.Set<StopEntity>()
            .Where(s => s.RouteId == request.RouteId && !s.IsDeleted)
            .ToListAsync(ct);

        if (stops.Count != request.Request.StopIds.Count)
            return Error.Validation($"Reorder must include all {stops.Count} stops of the route. Received {request.Request.StopIds.Count}.");

        var lookup = stops.ToDictionary(s => s.Id);
        foreach (var id in request.Request.StopIds)
            if (!lookup.ContainsKey(id))
                return Error.Validation($"Stop {id} does not belong to route {request.RouteId}.");

        // Phase 1: temporary negative orders
        for (int i = 0; i < stops.Count; i++)
            stops[i].StopOrder = -(i + 1);
        await _db.SaveChangesAsync(ct);

        // Phase 2: final orders
        var now = DateTime.UtcNow;
        for (int i = 0; i < request.Request.StopIds.Count; i++)
        {
            var stop = lookup[request.Request.StopIds[i]];
            stop.StopOrder = i + 1;
            stop.UpdatedAt = now;
        }
        await _db.SaveChangesAsync(ct);

        // TODO (audit): await _auditWriter.LogAsync("Stops", request.RouteId, FinancialAuditAction.Update, null, new { Reordered = true }, ct);

        var ordered = stops.OrderBy(s => s.StopOrder).Select(StopResponse.FromEntity).ToList();
        return Result.Success<IReadOnlyList<StopResponse>>(ordered);
    }
}
