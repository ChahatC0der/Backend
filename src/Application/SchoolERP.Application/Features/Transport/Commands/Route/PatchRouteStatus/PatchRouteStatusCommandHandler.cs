using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Fees.Builders;
using SchoolERP.Application.Features.Transport.DTOs.Route;
using SchoolERP.Domain.Shared.Results;
using SchoolERP.Domain.Transport.Enums;
using RouteEntity = SchoolERP.Domain.Transport.Entities.Route;

namespace SchoolERP.Application.Features.Transport.Commands.Route.PatchRouteStatus;

public sealed class PatchRouteStatusCommandHandler : IRequestHandler<PatchRouteStatusCommand, Result<RouteResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;

    public PatchRouteStatusCommandHandler(IApplicationDbContext db, ICurrentTenantService tenant, ICurrentBranchService branch)
    { _db = db; _tenant = tenant; _branch = branch; }

    public async Task<Result<RouteResponse>> Handle(PatchRouteStatusCommand request, CancellationToken ct)
    {
        var scope = TransportScopeGuard.RequireBranch(_tenant, _branch);
        if (scope.IsFailure) return scope.Error!;
        var (_, branchId) = scope.Value;

        var entity = await _db.Set<RouteEntity>().FirstOrDefaultAsync(r =>
            r.Id == request.Id && r.BranchId == branchId && !r.IsDeleted, ct);
        if (entity is null) return Error.NotFound("Route", request.Id.ToString());

        entity.Status = request.Request.Status.ToLowerInvariant() == "inactive"
            ? RouteStatus.Inactive : RouteStatus.Active;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        // TODO (audit): await _auditWriter.LogAsync("Routes", entity.Id, FinancialAuditAction.Update, null, new { Status = entity.Status }, ct);

        return Result.Success(RouteResponse.FromEntity(entity));
    }
}
