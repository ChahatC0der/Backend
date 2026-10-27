using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Fees.Builders;
using SchoolERP.Application.Features.Transport.Commands.Stop.CreateStop;
using SchoolERP.Application.Features.Transport.DTOs.Stop;
using SchoolERP.Domain.Shared.Results;
using RouteEntity = SchoolERP.Domain.Transport.Entities.Route;
using StopEntity = SchoolERP.Domain.Transport.Entities.Stop;

namespace SchoolERP.Application.Features.Transport.Commands.Stop.UpdateStop;

public sealed class UpdateStopCommandHandler : IRequestHandler<UpdateStopCommand, Result<StopResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;

    public UpdateStopCommandHandler(IApplicationDbContext db, ICurrentTenantService tenant, ICurrentBranchService branch)
    { _db = db; _tenant = tenant; _branch = branch; }

    public async Task<Result<StopResponse>> Handle(UpdateStopCommand request, CancellationToken ct)
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

        if (!CreateStopCommandHandler.TryParseStopType(request.Request.StopType, out var type))
            return Error.Validation($"Invalid StopType: {request.Request.StopType}.");

        // StopOrder conflict with other stop?
        if (entity.StopOrder != request.Request.StopOrder)
        {
            var clash = await _db.Set<StopEntity>().AnyAsync(s =>
                s.Id != request.StopId && s.RouteId == request.RouteId
                && !s.IsDeleted && s.StopOrder == request.Request.StopOrder, ct);
            if (clash) return Error.Conflict($"Stop order {request.Request.StopOrder} is already taken.");
        }

        entity.Name = request.Request.Name.Trim();
        entity.StopOrder = request.Request.StopOrder;
        entity.PickupTime = request.Request.PickupTime;
        entity.DropTime = request.Request.DropTime;
        entity.Landmark = request.Request.Landmark?.Trim();
        entity.DistanceFromCampus = request.Request.DistanceFromCampus;
        entity.StopType = type;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        // TODO (audit): await _auditWriter.LogAsync("Stops", entity.Id, FinancialAuditAction.Update, null, entity, ct);

        return Result.Success(StopResponse.FromEntity(entity));
    }
}
