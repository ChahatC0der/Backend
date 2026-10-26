using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Fees.Enums;
using SchoolERP.Domain.Shared.Results;
using FeeStructureEntity = SchoolERP.Domain.Fees.Entities.FeeStructure;

namespace SchoolERP.Application.Features.Fees.Commands.FeeStructure.DeleteFeeStructure;

public sealed class DeleteFeeStructureCommandHandler : IRequestHandler<DeleteFeeStructureCommand, Result<bool>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;

    public DeleteFeeStructureCommandHandler(IApplicationDbContext db, ICurrentTenantService tenant, ICurrentBranchService branch)
    {
        _db = db; _tenant = tenant; _branch = branch;
    }

    public async Task<Result<bool>> Handle(DeleteFeeStructureCommand request, CancellationToken ct)
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

        if (entity.Status == FeeStructureStatus.Active)
            return Error.Conflict("Active fee structures cannot be deleted. Archive first.");

        var now = DateTime.UtcNow;
        entity.DeletedAt = now;
        entity.UpdatedAt = now;
        foreach (var l in entity.Lines.Where(x => x.DeletedAt == null))
        {
            l.DeletedAt = now;
            l.UpdatedAt = now;
        }

        await _db.SaveChangesAsync(ct);

        // TODO (audit): await _auditWriter.LogAsync("FeeStructures", entity.Id, FinancialAuditAction.Delete, null, null, ct);

        return Result.Success(true);
    }
}
