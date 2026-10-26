using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Fees.DTOs.FeeHead;
using FeeHeadEntity = SchoolERP.Domain.Fees.Entities.FeeHead;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Fees.Commands.FeeHead.PatchFeeHead;

public sealed class PatchFeeHeadCommandHandler : IRequestHandler<PatchFeeHeadCommand, Result<FeeHeadResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;

    public PatchFeeHeadCommandHandler(IApplicationDbContext db, ICurrentTenantService tenant, ICurrentBranchService branch)
    {
        _db = db; _tenant = tenant; _branch = branch;
    }

    public async Task<Result<FeeHeadResponse>> Handle(PatchFeeHeadCommand request, CancellationToken ct)
    {
        var scope = FeeScopeGuard.RequireBranch(_tenant, _branch);
        if (scope.IsFailure) return scope.Error!;
        var (tenantId, branchId) = scope.Value;

        var entity = await _db.Set<FeeHeadEntity>().FirstOrDefaultAsync(x =>
            x.Id == request.Id && x.TenantId == tenantId &&
            x.BranchId == branchId && x.DeletedAt == null, ct);
        if (entity is null) return Error.NotFound("FeeHead", request.Id.ToString());

        if (!string.IsNullOrWhiteSpace(request.Request.Name))
            entity.Name = request.Request.Name.Trim();

        if (!string.IsNullOrWhiteSpace(request.Request.Description))
            entity.Description = request.Request.Description.Trim();

        if (!string.IsNullOrWhiteSpace(request.Request.Code) && request.Request.Code != entity.Code)
        {
            var duplicate = await _db.Set<FeeHeadEntity>().AnyAsync(x =>
                x.Id != request.Id && x.TenantId == tenantId && x.BranchId == branchId &&
                x.DeletedAt == null &&
                x.Code == request.Request.Code, ct);
            if (duplicate)
                return Error.Conflict($"A fee head with code '{request.Request.Code}' already exists.");
            entity.Code = request.Request.Code.Trim();
        }

        if (request.Request.IsRefundable.HasValue) entity.IsRefundable = request.Request.IsRefundable.Value;
        if (request.Request.IsMandatory.HasValue) entity.IsMandatory = request.Request.IsMandatory.Value;
        if (request.Request.IsOptional.HasValue) entity.IsOptional = request.Request.IsOptional.Value;

        entity.UpdatedAt = DateTime.UtcNow;

        // TODO (audit): await _auditWriter.LogAsync("FeeHeads", entity.Id, FinancialAuditAction.Update, null, entity, ct);
        return Result.Success(FeeHeadResponse.FromEntity(entity));
    }
}
