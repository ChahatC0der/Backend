using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Fees.Builders;
using SchoolERP.Application.Features.Transport.DTOs.Vehicle;
using SchoolERP.Domain.Shared.Results;
using SchoolERP.Domain.Transport.Enums;
using VehicleEntity = SchoolERP.Domain.Transport.Entities.Vehicle;

namespace SchoolERP.Application.Features.Transport.Commands.Vehicle.CreateVehicle;

public sealed class CreateVehicleCommandHandler : IRequestHandler<CreateVehicleCommand, Result<VehicleResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;

    public CreateVehicleCommandHandler(IApplicationDbContext db, ICurrentTenantService tenant, ICurrentBranchService branch)
    { _db = db; _tenant = tenant; _branch = branch; }

    public async Task<Result<VehicleResponse>> Handle(CreateVehicleCommand request, CancellationToken ct)
    {
        var scope = TransportScopeGuard.RequireBranch(_tenant, _branch);
        if (scope.IsFailure) return scope.Error!;
        var (tenantId, branchId) = scope.Value;

        if (!TryParseVehicleType(request.Request.VehicleType, out var type))
            return Error.Validation($"Invalid VehicleType: {request.Request.VehicleType}.");

        var normalizedNumber = request.Request.VehicleNumber.Trim().ToUpperInvariant();

        // --- Validations (all before mutation) ---
        var duplicate = await _db.Set<VehicleEntity>().AnyAsync(v =>
            v.TenantId == tenantId && v.BranchId == branchId
            && !v.IsDeleted
            && v.VehicleNumber == normalizedNumber, ct);
        if (duplicate)
            return Error.Conflict($"Vehicle number {normalizedNumber} already exists in this branch.");

        // --- Mutations ---
        var entity = new VehicleEntity
        {
            TenantId = tenantId,
            BranchId = branchId,
            VehicleNumber = normalizedNumber,
            VehicleType = type,
            Capacity = request.Request.Capacity,
            Model = request.Request.Model?.Trim(),
            Make = request.Request.Make?.Trim(),
            Year = request.Request.Year,
            InsuranceValidUntil = request.Request.InsuranceValidUntil,
            PucValidUntil = request.Request.PucValidUntil,
            FitnessValidUntil = request.Request.FitnessValidUntil,
            Status = VehicleStatus.Active,
            CreatedAt = DateTime.UtcNow,
        };
        _db.Set<VehicleEntity>().Add(entity);
        await _db.SaveChangesAsync(ct);

        // TODO (audit): await _auditWriter.LogAsync("Vehicles", entity.Id, FinancialAuditAction.Create, null, entity, ct);

        return Result.Success(VehicleResponse.FromEntity(entity));
    }

    internal static bool TryParseVehicleType(string value, out VehicleType type)
    {
        foreach (var kv in EnumMaps_JsonAdapter.VehicleTypeMap)
            if (string.Equals(kv.Value, value, StringComparison.OrdinalIgnoreCase)) { type = kv.Key; return true; }
        type = default; return false;
    }
}
