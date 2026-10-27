using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Fees.Builders;
using SchoolERP.Domain.Shared.Results;
using TransportFeeEntity = SchoolERP.Domain.Transport.Entities.TransportFee;

namespace SchoolERP.Application.Features.Transport.Commands.TransportFee.DeleteTransportFee;

public sealed class DeleteTransportFeeCommandHandler : IRequestHandler<DeleteTransportFeeCommand, Result<bool>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;

    public DeleteTransportFeeCommandHandler(IApplicationDbContext db, ICurrentTenantService tenant, ICurrentBranchService branch)
    { _db = db; _tenant = tenant; _branch = branch; }

    public async Task<Result<bool>> Handle(DeleteTransportFeeCommand request, CancellationToken ct)
    {
        var scope = TransportScopeGuard.RequireBranch(_tenant, _branch);
        if (scope.IsFailure) return scope.Error!;
        var (_, branchId) = scope.Value;

        var entity = await _db.Set<TransportFeeEntity>().FirstOrDefaultAsync(f =>
            f.Id == request.Id && f.BranchId == branchId && !f.IsDeleted, ct);
        if (entity is null) return Error.NotFound("TransportFee", request.Id.ToString());

        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        // TODO (audit): await _auditWriter.LogAsync("TransportFees", entity.Id, FinancialAuditAction.Delete, null, null, ct);

        return Result.Success(true);
    }
}
