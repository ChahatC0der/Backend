using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Fees.Enums;
using SchoolERP.Domain.Shared.Results;
using ConcessionEntity = SchoolERP.Domain.Fees.Entities.Concession;
using ConcessionAllocationEntity = SchoolERP.Domain.Fees.Entities.ConcessionAllocation;
using StudentEntity = SchoolERP.Domain.Student.Entities.Student;

namespace SchoolERP.Application.Features.Fees.Commands.Concession.AutoApplySiblingConcessions;

public sealed class AutoApplySiblingConcessionsCommandHandler : IRequestHandler<AutoApplySiblingConcessionsCommand, Result<AutoApplySiblingResult>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;
    private readonly ICurrentUserService _user;

    public AutoApplySiblingConcessionsCommandHandler(IApplicationDbContext db, ICurrentTenantService tenant,
                                                     ICurrentBranchService branch, ICurrentUserService user)
    { _db = db; _tenant = tenant; _branch = branch; _user = user; }

    public async Task<Result<AutoApplySiblingResult>> Handle(AutoApplySiblingConcessionsCommand request, CancellationToken ct)
    {
        var scope = FeeScopeGuard.RequireBranch(_tenant, _branch);
        if (scope.IsFailure) return scope.Error!;
        var (tenantId, branchId) = scope.Value;

        var siblingConcession = await _db.Set<ConcessionEntity>()
            .Where(c => c.TenantId == tenantId && c.BranchId == branchId &&
                        c.DeletedAt == null && c.Type == ConcessionType.Sibling)
            .OrderByDescending(c => c.CreatedAt)
            .FirstOrDefaultAsync(ct);
        if (siblingConcession is null)
            return Error.Validation("No sibling-type concession configured for this branch.");

        var familyGroups = await _db.Set<StudentEntity>().AsNoTracking()
            .Where(s => s.TenantId == tenantId && s.BranchId == branchId && s.FamilyId != null)
            .Select(s => new { s.Id, s.FamilyId })
            .ToListAsync(ct);

        var perFamily = familyGroups.GroupBy(x => x.FamilyId!.Value)
            .Where(g => g.Count() >= 2)
            .ToList();

        if (perFamily.Count == 0)
            return Result.Success(new AutoApplySiblingResult(0, 0, 0));

        var existing = await _db.Set<ConcessionAllocationEntity>()
            .Where(a => a.ConcessionId == siblingConcession.Id &&
                        a.AcademicYearId == request.AcademicYearId &&
                        (a.Status == ConcessionAllocationStatus.Pending || a.Status == ConcessionAllocationStatus.Approved))
            .Select(a => a.StudentId).ToListAsync(ct);
        var existingSet = existing.ToHashSet();

        var now = DateTime.UtcNow;
        var status = siblingConcession.ApprovalRequired ? ConcessionAllocationStatus.Pending : ConcessionAllocationStatus.Approved;
        var approvedBy = status == ConcessionAllocationStatus.Approved ? _user.GetUserId() : null;
        var approvedAt = status == ConcessionAllocationStatus.Approved ? now : (DateTime?)null;

        var toInsert = new List<ConcessionAllocationEntity>();
        var skipped = 0;
        foreach (var group in perFamily)
        {
            // Phase 1 rule: allocate to every sibling except the first (by Id order).
            var siblings = group.OrderBy(s => s.Id).Skip(1).ToList();
            foreach (var s in siblings)
            {
                if (existingSet.Contains(s.Id)) { skipped++; continue; }
                toInsert.Add(new ConcessionAllocationEntity
                {
                    ConcessionId = siblingConcession.Id,
                    StudentId = s.Id,
                    AcademicYearId = request.AcademicYearId,
                    Status = status,
                    ApprovedBy = approvedBy,
                    ApprovedAt = approvedAt,
                    CreatedAt = now,
                });
            }
        }

        _db.Set<ConcessionAllocationEntity>().AddRange(toInsert);
        await _db.SaveChangesAsync(ct);

        // TODO (audit): await _auditWriter.LogAsync("ConcessionAllocations", 0, FinancialAuditAction.Create, null, new { SiblingConcessionId = siblingConcession.Id, Count = toInsert.Count }, ct);
        return Result.Success(new AutoApplySiblingResult(perFamily.Count, toInsert.Count, skipped));
    }
}
