using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Fees.DTOs.FeeStructure;
using SchoolERP.Domain.Fees.Enums;
using SchoolERP.Domain.Shared.Results;
using FeeStructureEntity = SchoolERP.Domain.Fees.Entities.FeeStructure;

namespace SchoolERP.Application.Features.Fees.Commands.FeeStructure.PatchFeeStructureStatus;

public sealed class PatchFeeStructureStatusCommandHandler : IRequestHandler<PatchFeeStructureStatusCommand, Result<FeeStructureResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;

    public PatchFeeStructureStatusCommandHandler(IApplicationDbContext db, ICurrentTenantService tenant, ICurrentBranchService branch)
    {
        _db = db; _tenant = tenant; _branch = branch;
    }

    public async Task<Result<FeeStructureResponse>> Handle(PatchFeeStructureStatusCommand request, CancellationToken ct)
    {
        var scope = FeeScopeGuard.RequireBranch(_tenant, _branch);
        if (scope.IsFailure) return scope.Error!;
        var (tenantId, branchId) = scope.Value;

        var branchBelongs = await _db.Set<SchoolERP.Domain.Tenants.Entities.Branch>()
            .AnyAsync(b => b.Id == branchId && b.TenantId == tenantId, ct);
        if (!branchBelongs) return Error.Unauthorized("Branch does not belong to the current tenant.");

        var entity = await _db.Set<FeeStructureEntity>()
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.BranchId == branchId && x.DeletedAt == null, ct);
        if (entity is null) return Error.NotFound("FeeStructure", request.Id.ToString());

        var target = request.Request.Status switch
        {
            "draft"    => FeeStructureStatus.Draft,
            "active"   => FeeStructureStatus.Active,
            "archived" => FeeStructureStatus.Archived,
            _          => (FeeStructureStatus?)null,
        };
        if (target is null) return Error.Validation("Invalid status.");

        // Allowed transitions: Draft → Active, Draft → Archived, Active → Archived.
        // Reverse transitions (Active → Draft, Archived → anything) are Phase 2+ only.
        var valid = (entity.Status, target.Value) switch
        {
            (FeeStructureStatus.Draft, FeeStructureStatus.Active) => true,
            (FeeStructureStatus.Draft, FeeStructureStatus.Archived) => true,
            (FeeStructureStatus.Active, FeeStructureStatus.Archived) => true,
            _ => false,
        };
        if (!valid)
            return Error.Conflict($"Cannot transition from '{entity.Status}' to '{target}'.");

        entity.Status = target.Value;
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        // TODO (audit): await _auditWriter.LogAsync("FeeStructures", entity.Id, FinancialAuditAction.Update, null, new { Status = target.Value }, ct);

        return Result.Success(FeeStructureResponseFactory.ToResponse(entity));
    }
}
