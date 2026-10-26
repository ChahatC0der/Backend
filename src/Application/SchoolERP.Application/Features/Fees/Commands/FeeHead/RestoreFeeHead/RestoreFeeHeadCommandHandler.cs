using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.Interfaces;
using FeeHeadEntity = SchoolERP.Domain.Fees.Entities.FeeHead;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Fees.Commands.FeeHead.RestoreFeeHead;

public sealed class RestoreFeeHeadCommandHandler : IRequestHandler<RestoreFeeHeadCommand, Result<bool>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;

    public RestoreFeeHeadCommandHandler(IApplicationDbContext db, ICurrentTenantService tenant, ICurrentBranchService branch)
    {
        _db = db; _tenant = tenant; _branch = branch;
    }

    public async Task<Result<bool>> Handle(RestoreFeeHeadCommand request, CancellationToken ct)
    {
        var scope = FeeScopeGuard.RequireBranch(_tenant, _branch);
        if (scope.IsFailure) return scope.Error!;
        var (tenantId, branchId) = scope.Value;

        var entity = await _db.Set<FeeHeadEntity>().FirstOrDefaultAsync(x =>
            x.Id == request.Id && x.TenantId == tenantId &&
            x.BranchId == branchId && x.DeletedAt != null, ct);
        if (entity is null) return Error.NotFound("FeeHead (deleted)", request.Id.ToString());

        var clash = await _db.Set<FeeHeadEntity>().AnyAsync(x =>
            x.Id != request.Id && x.TenantId == tenantId && x.BranchId == branchId &&
            x.DeletedAt == null &&
            x.Code == entity.Code, ct);
        if (clash)
            return Error.Conflict($"Cannot restore: code '{entity.Code}' is in use by an active fee head.");

        entity.DeletedAt = null;
        entity.UpdatedAt = DateTime.UtcNow;

        // TODO (audit): await _auditWriter.LogAsync("FeeHeads", entity.Id, FinancialAuditAction.Restore, null, null, ct);
        return Result.Success(true);
    }
}
