using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Fees.Builders;
using SchoolERP.Application.Features.Transport.DTOs.Stop;
using SchoolERP.Domain.Shared.Results;
using SchoolERP.Domain.Transport.Enums;
using RouteEntity = SchoolERP.Domain.Transport.Entities.Route;
using StopEntity = SchoolERP.Domain.Transport.Entities.Stop;

namespace SchoolERP.Application.Features.Transport.Commands.Stop.CreateStop;

public sealed class CreateStopCommandHandler : IRequestHandler<CreateStopCommand, Result<StopResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;

    public CreateStopCommandHandler(IApplicationDbContext db, ICurrentTenantService tenant, ICurrentBranchService branch)
    { _db = db; _tenant = tenant; _branch = branch; }

    public async Task<Result<StopResponse>> Handle(CreateStopCommand request, CancellationToken ct)
    {
        var scope = TransportScopeGuard.RequireBranch(_tenant, _branch);
        if (scope.IsFailure) return scope.Error!;
        var (_, branchId) = scope.Value;

        // Verify route belongs to scope
        var routeOk = await _db.Set<RouteEntity>().AnyAsync(r =>
            r.Id == request.RouteId && r.BranchId == branchId && !r.IsDeleted, ct);
        if (!routeOk) return Error.NotFound("Route", request.RouteId.ToString());

        if (!TryParseStopType(request.Request.StopType, out var type))
            return Error.Validation($"Invalid StopType: {request.Request.StopType}.");

        // Check StopOrder conflict
        var clash = await _db.Set<StopEntity>().AnyAsync(s =>
            s.RouteId == request.RouteId && !s.IsDeleted && s.StopOrder == request.Request.StopOrder, ct);
        if (clash) return Error.Conflict($"Stop order {request.Request.StopOrder} is already taken in this route.");

        var entity = new StopEntity
        {
            RouteId = request.RouteId,
            Name = request.Request.Name.Trim(),
            StopOrder = request.Request.StopOrder,
            PickupTime = request.Request.PickupTime,
            DropTime = request.Request.DropTime,
            Landmark = request.Request.Landmark?.Trim(),
            DistanceFromCampus = request.Request.DistanceFromCampus,
            StopType = type,
            CreatedAt = DateTime.UtcNow,
        };
        _db.Set<StopEntity>().Add(entity);
        await _db.SaveChangesAsync(ct);

        // TODO (audit): await _auditWriter.LogAsync("Stops", entity.Id, FinancialAuditAction.Create, null, entity, ct);

        return Result.Success(StopResponse.FromEntity(entity));
    }

    internal static bool TryParseStopType(string value, out StopType type)
    {
        foreach (var kv in EnumMaps_JsonAdapter.StopTypeMap)
            if (string.Equals(kv.Value, value, StringComparison.OrdinalIgnoreCase)) { type = kv.Key; return true; }
        type = default; return false;
    }
}
