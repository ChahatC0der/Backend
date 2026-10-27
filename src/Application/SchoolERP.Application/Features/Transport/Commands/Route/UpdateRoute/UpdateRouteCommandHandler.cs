using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Fees.Builders;
using SchoolERP.Application.Features.Transport.DTOs.Route;
using SchoolERP.Domain.Shared.Results;
using RouteEntity = SchoolERP.Domain.Transport.Entities.Route;

namespace SchoolERP.Application.Features.Transport.Commands.Route.UpdateRoute;

public sealed class UpdateRouteCommandHandler : IRequestHandler<UpdateRouteCommand, Result<RouteResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;

    public UpdateRouteCommandHandler(IApplicationDbContext db, ICurrentTenantService tenant, ICurrentBranchService branch)
    { _db = db; _tenant = tenant; _branch = branch; }

    public async Task<Result<RouteResponse>> Handle(UpdateRouteCommand request, CancellationToken ct)
    {
        var scope = TransportScopeGuard.RequireBranch(_tenant, _branch);
        if (scope.IsFailure) return scope.Error!;
        var (_, branchId) = scope.Value;

        var entity = await _db.Set<RouteEntity>().FirstOrDefaultAsync(r =>
            r.Id == request.Id && r.BranchId == branchId && !r.IsDeleted, ct);
        if (entity is null) return Error.NotFound("Route", request.Id.ToString());

        var code = request.Request.Code.Trim().ToUpperInvariant();
        var duplicate = await _db.Set<RouteEntity>().AnyAsync(r =>
            r.Id != request.Id && r.BranchId == branchId && !r.IsDeleted && r.Code == code, ct);
        if (duplicate) return Error.Conflict($"Route code {code} already exists.");

        entity.Name = request.Request.Name.Trim();
        entity.Code = code;
        entity.Description = request.Request.Description?.Trim();
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        // TODO (audit): await _auditWriter.LogAsync("Routes", entity.Id, FinancialAuditAction.Update, null, entity, ct);

        return Result.Success(RouteResponse.FromEntity(entity));
    }
}
