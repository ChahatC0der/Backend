using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Fees.Builders;
using SchoolERP.Domain.Shared.Results;
using RouteEntity = SchoolERP.Domain.Transport.Entities.Route;
using StopEntity = SchoolERP.Domain.Transport.Entities.Stop;

namespace SchoolERP.Application.Features.Transport.Commands.Stop.DeleteStop;

public sealed class DeleteStopCommandHandler : IRequestHandler<DeleteStopCommand, Result<bool>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;

    public DeleteStopCommandHandler(IApplicationDbContext db, ICurrentTenantService tenant, ICurrentBranchService branch)
    { _db = db; _tenant = tenant; _branch = branch; }

    public async Task<Result<bool>> Handle(DeleteStopCommand request, CancellationToken ct)
    {
        var scope = TransportScopeGuard.RequireBranch(_tenant, _branch);
        if (scope.IsFailure) return scope.Error!;
        var (_, branchId) = scope.Value;

        var routeOk = await _db.Set<RouteEntity>().AnyAsync(r =>
            r.Id == request.RouteId && r.BranchId == branchId && !r.IsDeleted, ct);
        if (!routeOk) return Error.NotFound("Route", request.RouteId.ToString());

        var entity = await _db.Set<StopEntity>().FirstOrDefaultAsync(s =>
            s.Id == request.StopId && s.RouteId == request.RouteId && !s.IsDeleted, ct);
        if (entity is null) return Error.NotFound("Stop", request.StopId.ToString());

        // TODO (Phase 5): block if active TransportAssignment references this stop.

        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        // TODO (audit): await _auditWriter.LogAsync("Stops", entity.Id, FinancialAuditAction.Delete, null, null, ct);

        return Result.Success(true);
    }
}
