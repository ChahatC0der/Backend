using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Fees.Builders;
using SchoolERP.Application.Features.Transport.DTOs.Vehicle;
using SchoolERP.Domain.Shared.Results;
using SchoolERP.Domain.Transport.Enums;
using VehicleEntity = SchoolERP.Domain.Transport.Entities.Vehicle;

namespace SchoolERP.Application.Features.Transport.Commands.Vehicle.PatchVehicleStatus;

public sealed class PatchVehicleStatusCommandHandler : IRequestHandler<PatchVehicleStatusCommand, Result<VehicleResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;

    public PatchVehicleStatusCommandHandler(IApplicationDbContext db, ICurrentTenantService tenant, ICurrentBranchService branch)
    { _db = db; _tenant = tenant; _branch = branch; }

    public async Task<Result<VehicleResponse>> Handle(PatchVehicleStatusCommand request, CancellationToken ct)
    {
        var scope = TransportScopeGuard.RequireBranch(_tenant, _branch);
        if (scope.IsFailure) return scope.Error!;
        var (tenantId, branchId) = scope.Value;

        var entity = await _db.Set<VehicleEntity>().FirstOrDefaultAsync(v =>
            v.Id == request.Id && v.TenantId == tenantId
            && v.BranchId == branchId && !v.IsDeleted, ct);
        if (entity is null) return Error.NotFound("Vehicle", request.Id.ToString());

        if (entity.Status == VehicleStatus.Retired)
            return Error.Conflict("Retired vehicles cannot change status. Use Restore to reactivate a deleted vehicle.");

        VehicleStatus target = request.Request.Status.ToLowerInvariant() switch
        {
            "active" => VehicleStatus.Active,
            "maintenance" => VehicleStatus.Maintenance,
            "retired" => VehicleStatus.Retired,
            _ => VehicleStatus.Active,
        };

        entity.Status = target;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        // TODO (audit): await _auditWriter.LogAsync("Vehicles", entity.Id, FinancialAuditAction.Update, null, new { Status = target }, ct);

        return Result.Success(VehicleResponse.FromEntity(entity));
    }
}
