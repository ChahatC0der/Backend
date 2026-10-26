using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Fees.Commands.Concession.CreateConcession;
using SchoolERP.Application.Features.Fees.DTOs.Concession;
using SchoolERP.Domain.Shared.Results;
using ConcessionEntity = SchoolERP.Domain.Fees.Entities.Concession;
using FeeHeadEntity = SchoolERP.Domain.Fees.Entities.FeeHead;

namespace SchoolERP.Application.Features.Fees.Commands.Concession.UpdateConcession;

public sealed class UpdateConcessionCommandHandler : IRequestHandler<UpdateConcessionCommand, Result<ConcessionResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;

    public UpdateConcessionCommandHandler(IApplicationDbContext db, ICurrentTenantService tenant, ICurrentBranchService branch)
    { _db = db; _tenant = tenant; _branch = branch; }

    public async Task<Result<ConcessionResponse>> Handle(UpdateConcessionCommand request, CancellationToken ct)
    {
        var scope = FeeScopeGuard.RequireBranch(_tenant, _branch);
        if (scope.IsFailure) return scope.Error!;
        var (tenantId, branchId) = scope.Value;

        var entity = await _db.Set<ConcessionEntity>().FirstOrDefaultAsync(x =>
            x.Id == request.Id && x.TenantId == tenantId && x.BranchId == branchId && x.DeletedAt == null, ct);
        if (entity is null) return Error.NotFound("Concession", request.Id.ToString());

        if (!CreateConcessionCommandHandler.TryParseConcessionType(request.Request.Type, out var type))
            return Error.Validation($"Invalid concession Type '{request.Request.Type}'.");

        if (request.Request.FeeHeadId.HasValue)
        {
            var exists = await _db.Set<FeeHeadEntity>().AnyAsync(h =>
                h.Id == request.Request.FeeHeadId.Value &&
                h.TenantId == tenantId && h.BranchId == branchId && h.DeletedAt == null, ct);
            if (!exists) return Error.NotFound("FeeHead", request.Request.FeeHeadId.Value.ToString());
        }

        entity.Name = request.Request.Name.Trim();
        entity.Type = type;
        entity.Value = request.Request.Value;
        entity.FeeHeadId = request.Request.FeeHeadId;
        entity.IsAutoApply = request.Request.IsAutoApply;
        entity.ApprovalRequired = request.Request.ApprovalRequired;
        entity.StartDate = request.Request.StartDate;
        entity.EndDate = request.Request.EndDate;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        // TODO (audit): await _auditWriter.LogAsync("Concessions", entity.Id, FinancialAuditAction.Update, null, entity, ct);

        string? name = null;
        if (entity.FeeHeadId.HasValue)
            name = await _db.Set<FeeHeadEntity>().Where(h => h.Id == entity.FeeHeadId.Value).Select(h => h.Name).FirstOrDefaultAsync(ct);

        return Result.Success(ConcessionResponse.FromEntity(entity, name));
    }
}
