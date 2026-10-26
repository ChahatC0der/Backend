using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Shared.Results;
using ConcessionEntity = SchoolERP.Domain.Fees.Entities.Concession;

namespace SchoolERP.Application.Features.Fees.Commands.Concession.DeleteConcession;

public sealed class DeleteConcessionCommandHandler : IRequestHandler<DeleteConcessionCommand, Result<bool>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;

    public DeleteConcessionCommandHandler(IApplicationDbContext db, ICurrentTenantService tenant, ICurrentBranchService branch)
    { _db = db; _tenant = tenant; _branch = branch; }

    public async Task<Result<bool>> Handle(DeleteConcessionCommand request, CancellationToken ct)
    {
        var scope = FeeScopeGuard.RequireBranch(_tenant, _branch);
        if (scope.IsFailure) return scope.Error!;
        var (tenantId, branchId) = scope.Value;

        var entity = await _db.Set<ConcessionEntity>().FirstOrDefaultAsync(x =>
            x.Id == request.Id && x.TenantId == tenantId && x.BranchId == branchId && x.DeletedAt == null, ct);
        if (entity is null) return Error.NotFound("Concession", request.Id.ToString());

        entity.DeletedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        // TODO (audit): await _auditWriter.LogAsync("Concessions", entity.Id, FinancialAuditAction.Delete, null, null, ct);
        return Result.Success(true);
    }
}
