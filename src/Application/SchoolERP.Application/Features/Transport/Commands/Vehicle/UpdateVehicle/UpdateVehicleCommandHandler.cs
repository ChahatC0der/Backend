using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Fees.Builders;
using SchoolERP.Application.Features.Transport.Commands.Vehicle.CreateVehicle;
using SchoolERP.Application.Features.Transport.DTOs.Vehicle;
using SchoolERP.Domain.Shared.Results;
using VehicleEntity = SchoolERP.Domain.Transport.Entities.Vehicle;

namespace SchoolERP.Application.Features.Transport.Commands.Vehicle.UpdateVehicle;

public sealed class UpdateVehicleCommandHandler : IRequestHandler<UpdateVehicleCommand, Result<VehicleResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;

    public UpdateVehicleCommandHandler(IApplicationDbContext db, ICurrentTenantService tenant, ICurrentBranchService branch)
    { _db = db; _tenant = tenant; _branch = branch; }

    public async Task<Result<VehicleResponse>> Handle(UpdateVehicleCommand request, CancellationToken ct)
    {
        var scope = TransportScopeGuard.RequireBranch(_tenant, _branch);
        if (scope.IsFailure) return scope.Error!;
        var (tenantId, branchId) = scope.Value;

        var entity = await _db.Set<VehicleEntity>().FirstOrDefaultAsync(v =>
            v.Id == request.Id && v.TenantId == tenantId
            && v.BranchId == branchId && !v.IsDeleted, ct);
        if (entity is null) return Error.NotFound("Vehicle", request.Id.ToString());

        if (!CreateVehicleCommandHandler.TryParseVehicleType(request.Request.VehicleType, out var type))
            return Error.Validation($"Invalid VehicleType: {request.Request.VehicleType}.");

        var normalizedNumber = request.Request.VehicleNumber.Trim().ToUpperInvariant();

        var duplicate = await _db.Set<VehicleEntity>().AnyAsync(v =>
            v.Id != request.Id && v.TenantId == tenantId && v.BranchId == branchId
            && !v.IsDeleted && v.VehicleNumber == normalizedNumber, ct);
        if (duplicate) return Error.Conflict($"Vehicle number {normalizedNumber} already exists.");

        // --- Mutations ---
        entity.VehicleNumber = normalizedNumber;
        entity.VehicleType = type;
        entity.Capacity = request.Request.Capacity;
        entity.Model = request.Request.Model?.Trim();
        entity.Make = request.Request.Make?.Trim();
        entity.Year = request.Request.Year;
        entity.InsuranceValidUntil = request.Request.InsuranceValidUntil;
        entity.PucValidUntil = request.Request.PucValidUntil;
        entity.FitnessValidUntil = request.Request.FitnessValidUntil;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        // TODO (audit): await _auditWriter.LogAsync("Vehicles", entity.Id, FinancialAuditAction.Update, null, entity, ct);

        return Result.Success(VehicleResponse.FromEntity(entity));
    }
}
