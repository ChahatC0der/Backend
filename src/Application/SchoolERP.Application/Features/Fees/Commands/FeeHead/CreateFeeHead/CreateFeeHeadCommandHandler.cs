using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Fees.DTOs.FeeHead;
using FeeHeadEntity = SchoolERP.Domain.Fees.Entities.FeeHead;
using SchoolERP.Domain.Fees.Enums;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Fees.Commands.FeeHead.CreateFeeHead;

public sealed class CreateFeeHeadCommandHandler : IRequestHandler<CreateFeeHeadCommand, Result<FeeHeadResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;

    public CreateFeeHeadCommandHandler(IApplicationDbContext db, ICurrentTenantService tenant, ICurrentBranchService branch)
    {
        _db = db; _tenant = tenant; _branch = branch;
    }

    public async Task<Result<FeeHeadResponse>> Handle(CreateFeeHeadCommand request, CancellationToken ct)
    {
        var scope = FeeScopeGuard.RequireBranch(_tenant, _branch);
        if (scope.IsFailure) return scope.Error!;
        var (tenantId, branchId) = scope.Value;

        // --- All validations BEFORE any mutation ---
        var duplicate = await _db.Set<FeeHeadEntity>().AnyAsync(x =>
            x.TenantId == tenantId && x.BranchId == branchId &&
            x.DeletedAt == null &&
            x.Code == request.Request.Code, ct);
        if (duplicate)
            return Error.Conflict($"A fee head with code '{request.Request.Code}' already exists in this branch.");

        // --- Mutations ---
        var entity = new FeeHeadEntity
        {
            TenantId = tenantId,
            BranchId = branchId,
            Name = request.Request.Name.Trim(),
            Code = request.Request.Code.Trim(),
            Description = request.Request.Description?.Trim(),
            IsRefundable = request.Request.IsRefundable,
            IsMandatory = request.Request.IsMandatory,
            IsOptional = request.Request.IsOptional,
            Status = FeeHeadStatus.Active,
            CreatedAt = DateTime.UtcNow,
        };
        _db.Set<FeeHeadEntity>().Add(entity);
        await _db.SaveChangesAsync(ct);   // populate Id before building response

        // TODO (audit): await _auditWriter.LogAsync("FeeHeads", entity.Id, FinancialAuditAction.Create, null, entity, ct);

        return Result.Success(FeeHeadResponse.FromEntity(entity));
    }
}
