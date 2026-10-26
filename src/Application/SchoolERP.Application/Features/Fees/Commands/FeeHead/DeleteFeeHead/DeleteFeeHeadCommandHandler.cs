using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.Interfaces;
using FeeHeadEntity = SchoolERP.Domain.Fees.Entities.FeeHead;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Fees.Commands.FeeHead.DeleteFeeHead;

public sealed class DeleteFeeHeadCommandHandler : IRequestHandler<DeleteFeeHeadCommand, Result<bool>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;

    public DeleteFeeHeadCommandHandler(IApplicationDbContext db, ICurrentTenantService tenant, ICurrentBranchService branch)
    {
        _db = db; _tenant = tenant; _branch = branch;
    }

    public async Task<Result<bool>> Handle(DeleteFeeHeadCommand request, CancellationToken ct)
    {
        var scope = FeeScopeGuard.RequireBranch(_tenant, _branch);
        if (scope.IsFailure) return scope.Error!;
        var (tenantId, branchId) = scope.Value;

        var entity = await _db.Set<FeeHeadEntity>().FirstOrDefaultAsync(x =>
            x.Id == request.Id && x.TenantId == tenantId &&
            x.BranchId == branchId && x.DeletedAt == null, ct);
        if (entity is null) return Error.NotFound("FeeHead", request.Id.ToString());

        entity.DeletedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;

        // TODO (audit): await _auditWriter.LogAsync("FeeHeads", entity.Id, FinancialAuditAction.Delete, null, null, ct);
        return Result.Success(true);
    }
}
