using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Fees.DTOs.FeeStructure;
using SchoolERP.Domain.Shared.Results;
using FeeStructureEntity = SchoolERP.Domain.Fees.Entities.FeeStructure;
using StudentFeeAssignmentEntity = SchoolERP.Domain.Fees.Entities.StudentFeeAssignment;

namespace SchoolERP.Application.Features.Fees.Commands.FeeStructure.AssignStudentFeeStructure;

public sealed class AssignStudentFeeStructureCommandHandler : IRequestHandler<AssignStudentFeeStructureCommand, Result<StudentAssignedStructureResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;

    public AssignStudentFeeStructureCommandHandler(IApplicationDbContext db, ICurrentTenantService tenant, ICurrentBranchService branch)
    {
        _db = db; _tenant = tenant; _branch = branch;
    }

    public async Task<Result<StudentAssignedStructureResponse>> Handle(AssignStudentFeeStructureCommand request, CancellationToken ct)
    {
        var scope = FeeScopeGuard.RequireBranch(_tenant, _branch);
        if (scope.IsFailure) return scope.Error!;
        var (tenantId, branchId) = scope.Value;

        var branchBelongs = await _db.Set<SchoolERP.Domain.Tenants.Entities.Branch>()
            .AnyAsync(b => b.Id == branchId && b.TenantId == tenantId, ct);
        if (!branchBelongs) return Error.Unauthorized("Branch does not belong to the current tenant.");

        var structure = await _db.Set<FeeStructureEntity>()
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.Id == request.Request.FeeStructureId
                                   && x.BranchId == branchId
                                   && x.DeletedAt == null, ct);
        if (structure is null) return Error.NotFound("FeeStructure", request.Request.FeeStructureId.ToString());

        if (structure.AcademicYearId != request.Request.AcademicYearId)
            return Error.Validation("FeeStructure does not belong to the requested academic year.");

        // Deactivate existing active assignment for this student+AY
        var existing = await _db.Set<StudentFeeAssignmentEntity>()
            .Where(x => x.StudentId == request.Request.StudentId && x.AcademicYearId == request.Request.AcademicYearId && x.IsActive)
            .ToListAsync(ct);
        var now = DateTime.UtcNow;
        foreach (var e in existing) { e.IsActive = false; e.UpdatedAt = now; }

        _db.Set<StudentFeeAssignmentEntity>().Add(new StudentFeeAssignmentEntity
        {
            StudentId = request.Request.StudentId,
            FeeStructureId = request.Request.FeeStructureId,
            AcademicYearId = request.Request.AcademicYearId,
            OverrideAmount = request.Request.OverrideAmount,
            IsActive = true,
            CreatedAt = now,
        });

        await _db.SaveChangesAsync(ct);

        // TODO (audit): await _auditWriter.LogAsync("StudentFeeAssignments", request.Request.StudentId, FinancialAuditAction.Create, null, request.Request, ct);

        return Result.Success(new StudentAssignedStructureResponse
        {
            StudentId = request.Request.StudentId,
            AcademicYearId = request.Request.AcademicYearId,
            FeeStructureId = structure.Id,
            StructureName = structure.Name,
            ResolvedFrom = "override",
            OverrideAmount = request.Request.OverrideAmount,
            Lines = structure.Lines.Where(l => l.DeletedAt == null).Select(l => FeeStructureResponseFactory.ToLineResponse(l)).ToList(),
        });
    }
}
