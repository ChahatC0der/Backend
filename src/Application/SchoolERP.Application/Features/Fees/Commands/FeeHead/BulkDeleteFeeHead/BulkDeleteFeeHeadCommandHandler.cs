using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.Interfaces;
using FeeHeadEntity = SchoolERP.Domain.Fees.Entities.FeeHead;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Fees.Commands.FeeHead.BulkDeleteFeeHead;

public sealed class BulkDeleteFeeHeadCommandHandler : IRequestHandler<BulkDeleteFeeHeadCommand, Result<int>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;

    public BulkDeleteFeeHeadCommandHandler(IApplicationDbContext db, ICurrentTenantService tenant, ICurrentBranchService branch)
    {
        _db = db; _tenant = tenant; _branch = branch;
    }

    public async Task<Result<int>> Handle(BulkDeleteFeeHeadCommand request, CancellationToken ct)
    {
        var scope = FeeScopeGuard.RequireBranch(_tenant, _branch);
        if (scope.IsFailure) return scope.Error!;
        var (tenantId, branchId) = scope.Value;

        var ids = request.Ids.Distinct().ToList();
        var entities = await _db.Set<FeeHeadEntity>().Where(x =>
            ids.Contains(x.Id) && x.TenantId == tenantId &&
            x.BranchId == branchId && x.DeletedAt == null).ToListAsync(ct);

        var now = DateTime.UtcNow;
        foreach (var e in entities) { e.DeletedAt = now; e.UpdatedAt = now; }

        // TODO (audit): foreach (var e in entities) await _auditWriter.LogAsync("FeeHeads", e.Id, FinancialAuditAction.Delete, null, null, ct);
        return Result.Success(entities.Count);
    }
}
