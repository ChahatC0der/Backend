using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Fees.DTOs.Concession;
using SchoolERP.Domain.Fees.Enums;
using SchoolERP.Domain.Shared.Results;
using ConcessionEntity = SchoolERP.Domain.Fees.Entities.Concession;
using ConcessionAllocationEntity = SchoolERP.Domain.Fees.Entities.ConcessionAllocation;

namespace SchoolERP.Application.Features.Fees.Commands.Concession.ApproveConcession;

public sealed class ApproveConcessionCommandHandler : IRequestHandler<ApproveConcessionCommand, Result<ConcessionAllocationResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;
    private readonly ICurrentUserService _user;

    public ApproveConcessionCommandHandler(IApplicationDbContext db, ICurrentTenantService tenant,
                                           ICurrentBranchService branch, ICurrentUserService user)
    { _db = db; _tenant = tenant; _branch = branch; _user = user; }

    public async Task<Result<ConcessionAllocationResponse>> Handle(ApproveConcessionCommand request, CancellationToken ct)
    {
        var scope = FeeScopeGuard.RequireBranch(_tenant, _branch);
        if (scope.IsFailure) return scope.Error!;
        var (tenantId, branchId) = scope.Value;

        var alloc = await _db.Set<ConcessionAllocationEntity>().FirstOrDefaultAsync(a => a.Id == request.AllocationId, ct);
        if (alloc is null) return Error.NotFound("ConcessionAllocation", request.AllocationId.ToString());

        var concession = await _db.Set<ConcessionEntity>().FirstOrDefaultAsync(c =>
            c.Id == alloc.ConcessionId && c.TenantId == tenantId && c.BranchId == branchId && c.DeletedAt == null, ct);
        if (concession is null) return Error.Unauthorized("Concession does not belong to the current tenant/branch.");

        if (alloc.Status != ConcessionAllocationStatus.Pending)
            return Error.Conflict($"Cannot approve an allocation in state '{alloc.Status}'.");

        alloc.Status = ConcessionAllocationStatus.Approved;
        alloc.ApprovedBy = _user.GetUserId();
        alloc.ApprovedAt = DateTime.UtcNow;
        alloc.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        // TODO (audit): await _auditWriter.LogAsync("ConcessionAllocations", alloc.Id, FinancialAuditAction.Update, null, new { Status = "approved" }, ct);

        return Result.Success(new ConcessionAllocationResponse
        {
            Id = alloc.Id, ConcessionId = alloc.ConcessionId, ConcessionName = concession.Name,
            StudentId = alloc.StudentId, AcademicYearId = alloc.AcademicYearId,
            Status = alloc.Status.ToString(),
            ApprovedBy = alloc.ApprovedBy, ApprovedAt = alloc.ApprovedAt,
            RejectionReason = alloc.RejectionReason,
            CreatedAt = alloc.CreatedAt, UpdatedAt = alloc.UpdatedAt,
        });
    }
}
