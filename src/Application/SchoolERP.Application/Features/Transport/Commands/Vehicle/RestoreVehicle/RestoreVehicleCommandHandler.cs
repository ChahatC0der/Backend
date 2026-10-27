using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Fees.Builders;
using SchoolERP.Domain.Shared.Results;
using VehicleEntity = SchoolERP.Domain.Transport.Entities.Vehicle;

namespace SchoolERP.Application.Features.Transport.Commands.Vehicle.RestoreVehicle;

public sealed class RestoreVehicleCommandHandler : IRequestHandler<RestoreVehicleCommand, Result<bool>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;

    public RestoreVehicleCommandHandler(IApplicationDbContext db, ICurrentTenantService tenant, ICurrentBranchService branch)
    { _db = db; _tenant = tenant; _branch = branch; }

    public async Task<Result<bool>> Handle(RestoreVehicleCommand request, CancellationToken ct)
    {
        var scope = TransportScopeGuard.RequireBranch(_tenant, _branch);
        if (scope.IsFailure) return scope.Error!;
        var (tenantId, branchId) = scope.Value;

        var entity = await _db.Set<VehicleEntity>().FirstOrDefaultAsync(v =>
            v.Id == request.Id && v.TenantId == tenantId
            && v.BranchId == branchId && v.IsDeleted, ct);
        if (entity is null) return Error.NotFound("Vehicle (deleted)", request.Id.ToString());

        var clash = await _db.Set<VehicleEntity>().AnyAsync(v =>
            v.Id != request.Id && v.TenantId == tenantId && v.BranchId == branchId
            && !v.IsDeleted && v.VehicleNumber == entity.VehicleNumber, ct);
        if (clash)
            return Error.Conflict($"Cannot restore: vehicle number {entity.VehicleNumber} is in use by an active vehicle.");

        entity.IsDeleted = false;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        // TODO (audit): await _auditWriter.LogAsync("Vehicles", entity.Id, FinancialAuditAction.Restore, null, null, ct);

        return Result.Success(true);
    }
}
