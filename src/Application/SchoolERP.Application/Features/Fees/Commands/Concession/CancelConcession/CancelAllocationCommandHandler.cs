using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Shared.Results;
using ConcessionAllocationEntity = SchoolERP.Domain.Fees.Entities.ConcessionAllocation;

namespace SchoolERP.Application.Features.Fees.Commands.Concession.CancelAllocation;

public sealed class CancelAllocationCommandHandler
    : IRequestHandler<CancelAllocationCommand, Result<string>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;

    public CancelAllocationCommandHandler(IApplicationDbContext db, ICurrentTenantService tenant, ICurrentBranchService branch)
    { _db = db; _tenant = tenant; _branch = branch; }

    public async Task<Result<string>> Handle(CancelAllocationCommand request, CancellationToken ct)
    {
        var scope = FeeScopeGuard.RequireBranch(_tenant, _branch);
        if (scope.IsFailure) return scope.Error!;

        var allocation = await _db.Set<ConcessionAllocationEntity>()
            .FirstOrDefaultAsync(x => x.Id == request.Id, ct);

        if (allocation is null)
            return Error.NotFound("AllocationNotFound", $"Allocation '{request.Id}' not found.");

        _db.Set<ConcessionAllocationEntity>().Remove(allocation);
        await _db.SaveChangesAsync(ct);

        return Result.Success($"Allocation {request.Id} deleted successfully.");
    }
}