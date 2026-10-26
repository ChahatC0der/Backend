using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Fees.Builders;
using SchoolERP.Application.Features.Fees.DTOs.Concession;
using SchoolERP.Domain.Fees.Enums;
using SchoolERP.Domain.Shared.Results;
using ConcessionEntity = SchoolERP.Domain.Fees.Entities.Concession;
using FeeHeadEntity = SchoolERP.Domain.Fees.Entities.FeeHead;

namespace SchoolERP.Application.Features.Fees.Commands.Concession.CreateConcession;

public sealed class CreateConcessionCommandHandler : IRequestHandler<CreateConcessionCommand, Result<ConcessionResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;

    public CreateConcessionCommandHandler(IApplicationDbContext db, ICurrentTenantService tenant, ICurrentBranchService branch)
    { _db = db; _tenant = tenant; _branch = branch; }

    public async Task<Result<ConcessionResponse>> Handle(CreateConcessionCommand request, CancellationToken ct)
    {
        var scope = FeeScopeGuard.RequireBranch(_tenant, _branch);
        if (scope.IsFailure) return scope.Error!;
        var (tenantId, branchId) = scope.Value;

        if (!TryParseConcessionType(request.Request.Type, out var type))
            return Error.Validation($"Invalid concession Type '{request.Request.Type}'.");

        if (request.Request.FeeHeadId.HasValue)
        {
            var exists = await _db.Set<FeeHeadEntity>().AnyAsync(h =>
                h.Id == request.Request.FeeHeadId.Value &&
                h.TenantId == tenantId && h.BranchId == branchId && h.DeletedAt == null, ct);
            if (!exists) return Error.NotFound("FeeHead", request.Request.FeeHeadId.Value.ToString());
        }

        var entity = new ConcessionEntity
        {
            TenantId = tenantId,
            BranchId = branchId,
            Name = request.Request.Name.Trim(),
            Type = type,
            Value = request.Request.Value,
            FeeHeadId = request.Request.FeeHeadId,
            IsAutoApply = request.Request.IsAutoApply,
            ApprovalRequired = request.Request.ApprovalRequired,
            StartDate = request.Request.StartDate,
            EndDate = request.Request.EndDate,
            CreatedAt = DateTime.UtcNow,
        };
        _db.Set<ConcessionEntity>().Add(entity);
        await _db.SaveChangesAsync(ct);

        // TODO (audit): await _auditWriter.LogAsync("Concessions", entity.Id, FinancialAuditAction.Create, null, entity, ct);

        string? feeHeadName = null;
        if (entity.FeeHeadId.HasValue)
            feeHeadName = await _db.Set<FeeHeadEntity>().Where(h => h.Id == entity.FeeHeadId.Value).Select(h => h.Name).FirstOrDefaultAsync(ct);

        return Result.Success(ConcessionResponse.FromEntity(entity, feeHeadName));
    }

    internal static bool TryParseConcessionType(string value, out ConcessionType type)
    {
        foreach (var kv in EnumMaps_JsonAdapter.ConcessionTypeMap)
            if (string.Equals(kv.Value, value, StringComparison.OrdinalIgnoreCase)) { type = kv.Key; return true; }
        type = default; return false;
    }
}
