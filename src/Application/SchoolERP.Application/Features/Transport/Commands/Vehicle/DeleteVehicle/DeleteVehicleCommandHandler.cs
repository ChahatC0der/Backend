using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Fees.Builders;
using SchoolERP.Domain.Shared.Results;
using VehicleEntity = SchoolERP.Domain.Transport.Entities.Vehicle;

namespace SchoolERP.Application.Features.Transport.Commands.Vehicle.DeleteVehicle;

public sealed class DeleteVehicleCommandHandler : IRequestHandler<DeleteVehicleCommand, Result<bool>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;

    public DeleteVehicleCommandHandler(IApplicationDbContext db, ICurrentTenantService tenant, ICurrentBranchService branch)
    { _db = db; _tenant = tenant; _branch = branch; }

    public async Task<Result<bool>> Handle(DeleteVehicleCommand request, CancellationToken ct)
    {
        var scope = TransportScopeGuard.RequireBranch(_tenant, _branch);
        if (scope.IsFailure) return scope.Error!;
        var (tenantId, branchId) = scope.Value;

        var entity = await _db.Set<VehicleEntity>().FirstOrDefaultAsync(v =>
            v.Id == request.Id && v.TenantId == tenantId
            && v.BranchId == branchId && !v.IsDeleted, ct);
        if (entity is null) return Error.NotFound("Vehicle", request.Id.ToString());

        // TODO (Phase 3/5): block if active RouteVehicles / DriverAssignments / TransportAssignments exist.
        // Deferred until those entities are added. For now, soft delete proceeds.

        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        // TODO (audit): await _auditWriter.LogAsync("Vehicles", entity.Id, FinancialAuditAction.Delete, null, null, ct);

        return Result.Success(true);
    }
}
