using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Fees.Enums;
using SchoolERP.Domain.Shared.Results;
using ConcessionEntity = SchoolERP.Domain.Fees.Entities.Concession;
using ConcessionAllocationEntity = SchoolERP.Domain.Fees.Entities.ConcessionAllocation;
using StudentEnrollmentEntity = SchoolERP.Domain.Student.Entities.StudentEnrollment;

namespace SchoolERP.Application.Features.Fees.Commands.Concession.AllocateConcession;

public sealed class AllocateConcessionCommandHandler : IRequestHandler<AllocateConcessionCommand, Result<AllocateConcessionResult>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;
    private readonly ICurrentUserService _user;

    public AllocateConcessionCommandHandler(IApplicationDbContext db, ICurrentTenantService tenant,
                                            ICurrentBranchService branch, ICurrentUserService user)
    { _db = db; _tenant = tenant; _branch = branch; _user = user; }

    public async Task<Result<AllocateConcessionResult>> Handle(AllocateConcessionCommand request, CancellationToken ct)
    {
        var scope = FeeScopeGuard.RequireBranch(_tenant, _branch);
        if (scope.IsFailure) return scope.Error!;
        var (tenantId, branchId) = scope.Value;

        var concession = await _db.Set<ConcessionEntity>().FirstOrDefaultAsync(c =>
            c.Id == request.Request.ConcessionId && c.TenantId == tenantId && c.BranchId == branchId && c.DeletedAt == null, ct);
        if (concession is null) return Error.NotFound("Concession", request.Request.ConcessionId.ToString());

        List<long> studentIds;
        if (request.Request.StudentId.HasValue)
        {
            studentIds = new List<long> { request.Request.StudentId.Value };
        }
        else
        {
            // ClassId/SectionId live on StudentEnrollment, not Student.
            var q = _db.Set<StudentEnrollmentEntity>().AsNoTracking()
                .Where(e => e.AcademicYearId == request.Request.AcademicYearId
                         && e.ClassId == request.Request.ClassId!.Value);
            if (request.Request.SectionId.HasValue)
                q = q.Where(e => e.SectionId == request.Request.SectionId.Value);

            studentIds = await q.Select(e => e.StudentId).Distinct().Take(501).ToListAsync(ct);
            if (studentIds.Count > 500)
                return Error.Validation("Bulk allocation is capped at 500 students per request in Phase 1.");
        }

        if (studentIds.Count == 0)
            return Result.Success(new AllocateConcessionResult(0, 0));

        var existing = await _db.Set<ConcessionAllocationEntity>()
            .Where(a => a.ConcessionId == concession.Id && a.AcademicYearId == request.Request.AcademicYearId &&
                        studentIds.Contains(a.StudentId) &&
                        (a.Status == ConcessionAllocationStatus.Pending || a.Status == ConcessionAllocationStatus.Approved))
            .Select(a => a.StudentId).ToListAsync(ct);
        var existingSet = existing.ToHashSet();

        var now = DateTime.UtcNow;
        var status = concession.ApprovalRequired ? ConcessionAllocationStatus.Pending : ConcessionAllocationStatus.Approved;
        var approvedBy = status == ConcessionAllocationStatus.Approved ? _user.GetUserId() : null;
        var approvedAt = status == ConcessionAllocationStatus.Approved ? now : (DateTime?)null;

        var toInsert = new List<ConcessionAllocationEntity>();
        foreach (var sid in studentIds.Distinct())
        {
            if (existingSet.Contains(sid)) continue;
            toInsert.Add(new ConcessionAllocationEntity
            {
                ConcessionId = concession.Id,
                StudentId = sid,
                AcademicYearId = request.Request.AcademicYearId,
                Status = status,
                ApprovedBy = approvedBy,
                ApprovedAt = approvedAt,
                CreatedAt = now,
            });
        }

        _db.Set<ConcessionAllocationEntity>().AddRange(toInsert);
        await _db.SaveChangesAsync(ct);

        // TODO (audit): await _auditWriter.LogAsync("ConcessionAllocations", 0, FinancialAuditAction.Create, null, new { ConcessionId = concession.Id, Count = toInsert.Count }, ct);
        return Result.Success(new AllocateConcessionResult(toInsert.Count, existingSet.Count));
    }
}
