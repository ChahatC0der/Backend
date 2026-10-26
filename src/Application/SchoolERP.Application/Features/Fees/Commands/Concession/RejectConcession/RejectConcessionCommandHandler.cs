using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Fees.DTOs.Concession;
using SchoolERP.Domain.Fees.Enums;
using SchoolERP.Domain.Shared.Results;
using ConcessionEntity = SchoolERP.Domain.Fees.Entities.Concession;
using ConcessionAllocationEntity = SchoolERP.Domain.Fees.Entities.ConcessionAllocation;

namespace SchoolERP.Application.Features.Fees.Commands.Concession.RejectConcession;

public sealed class RejectConcessionCommandHandler : IRequestHandler<RejectConcessionCommand, Result<ConcessionAllocationResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;

    public RejectConcessionCommandHandler(IApplicationDbContext db, ICurrentTenantService tenant, ICurrentBranchService branch)
    { _db = db; _tenant = tenant; _branch = branch; }

    public async Task<Result<ConcessionAllocationResponse>> Handle(RejectConcessionCommand request, CancellationToken ct)
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
            return Error.Conflict($"Cannot reject an allocation in state '{alloc.Status}'.");

        alloc.Status = ConcessionAllocationStatus.Rejected;
        alloc.RejectionReason = request.Request.RejectionReason.Trim();
        alloc.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        // TODO (audit): await _auditWriter.LogAsync("ConcessionAllocations", alloc.Id, FinancialAuditAction.Update, null, new { Status = "rejected" }, ct);

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
