using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Fees.Builders;
using SchoolERP.Domain.Shared.Results;
using RouteEntity = SchoolERP.Domain.Transport.Entities.Route;
using StopEntity = SchoolERP.Domain.Transport.Entities.Stop;

namespace SchoolERP.Application.Features.Transport.Commands.Route.DeleteRoute;

public sealed class DeleteRouteCommandHandler : IRequestHandler<DeleteRouteCommand, Result<bool>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;

    public DeleteRouteCommandHandler(IApplicationDbContext db, ICurrentTenantService tenant, ICurrentBranchService branch)
    { _db = db; _tenant = tenant; _branch = branch; }

    public async Task<Result<bool>> Handle(DeleteRouteCommand request, CancellationToken ct)
    {
        var scope = TransportScopeGuard.RequireBranch(_tenant, _branch);
        if (scope.IsFailure) return scope.Error!;
        var (_, branchId) = scope.Value;

        var entity = await _db.Set<RouteEntity>().FirstOrDefaultAsync(r =>
            r.Id == request.Id && r.BranchId == branchId && !r.IsDeleted, ct);
        if (entity is null) return Error.NotFound("Route", request.Id.ToString());

        // Soft-delete all active stops of this route (cascade-equivalent)
        var stops = await _db.Set<StopEntity>().Where(s => s.RouteId == entity.Id && !s.IsDeleted).ToListAsync(ct);
        var now = DateTime.UtcNow;
        foreach (var s in stops) { s.IsDeleted = true; s.UpdatedAt = now; }

        entity.IsDeleted = true;
        entity.UpdatedAt = now;

        await _db.SaveChangesAsync(ct);
        // TODO (audit): await _auditWriter.LogAsync("Routes", entity.Id, FinancialAuditAction.Delete, null, null, ct);

        return Result.Success(true);
    }
}
