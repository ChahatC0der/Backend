using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Fees.DTOs.FeeStructure;
using SchoolERP.Domain.Fees.Enums;
using SchoolERP.Domain.Shared.Results;
using FeeStructureEntity = SchoolERP.Domain.Fees.Entities.FeeStructure;
using StudentFeeAssignmentEntity = SchoolERP.Domain.Fees.Entities.StudentFeeAssignment;
using StudentEntity = SchoolERP.Domain.Student.Entities.Student;
using StudentEnrollmentEntity = SchoolERP.Domain.Student.Entities.StudentEnrollment;

namespace SchoolERP.Application.Features.Fees.Queries.FeeStructure.GetStudentAssignedStructure;

public sealed class GetStudentAssignedStructureQueryHandler : IRequestHandler<GetStudentAssignedStructureQuery, Result<StudentAssignedStructureResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;

    public GetStudentAssignedStructureQueryHandler(IApplicationDbContext db, ICurrentTenantService tenant, ICurrentBranchService branch)
    { _db = db; _tenant = tenant; _branch = branch; }

    public async Task<Result<StudentAssignedStructureResponse>> Handle(GetStudentAssignedStructureQuery request, CancellationToken ct)
    {
        var scope = FeeScopeGuard.RequireBranch(_tenant, _branch);
        if (scope.IsFailure) return scope.Error!;
        var (_, branchId) = scope.Value;

        var studentExists = await _db.Set<StudentEntity>().AsNoTracking()
            .AnyAsync(s => s.Id == request.StudentId, ct);
        if (!studentExists) return Error.NotFound("Student", request.StudentId.ToString());

        // 1) Student-specific override takes precedence
        var activeOverride = await _db.Set<StudentFeeAssignmentEntity>().AsNoTracking()
            .Where(a => a.StudentId == request.StudentId && a.AcademicYearId == request.AcademicYearId && a.IsActive)
            .Select(a => new { a.FeeStructureId, a.OverrideAmount })
            .FirstOrDefaultAsync(ct);

        FeeStructureEntity? structure = null;
        var resolvedFrom = "none";
        decimal? overrideAmount = null;

        if (activeOverride is not null)
        {
            structure = await _db.Set<FeeStructureEntity>().AsNoTracking().Include(x => x.Lines)
                .FirstOrDefaultAsync(x => x.Id == activeOverride.FeeStructureId && x.DeletedAt == null, ct);
            if (structure is not null)
            {
                resolvedFrom = "override";
                overrideAmount = activeOverride.OverrideAmount;
            }
        }

        // 2) Fall back to class/section/classgroup resolution via StudentEnrollment
        if (structure is null)
        {
            // ClassId/SectionId live on StudentEnrollment, not Student
            var enrollment = await _db.Set<StudentEnrollmentEntity>().AsNoTracking()
                .Where(e => e.StudentId == request.StudentId && e.AcademicYearId == request.AcademicYearId)
                .OrderByDescending(e => e.Id)
                .Select(e => new { e.ClassId, e.SectionId })
                .FirstOrDefaultAsync(ct);

            if (enrollment is null)
            {
                return Result.Success(new StudentAssignedStructureResponse
                {
                    StudentId = request.StudentId,
                    AcademicYearId = request.AcademicYearId,
                    ResolvedFrom = "none",
                });
            }

            var candidates = await _db.Set<FeeStructureEntity>().AsNoTracking().Include(x => x.Lines)
                .Where(x => x.BranchId == branchId && x.AcademicYearId == request.AcademicYearId
                         && x.DeletedAt == null && x.Status == FeeStructureStatus.Active)
                .ToListAsync(ct);

            structure = candidates
                .Where(x => x.ClassId == enrollment.ClassId && x.SectionId == enrollment.SectionId)
                .OrderByDescending(x => x.SectionId.HasValue)
                .FirstOrDefault();
            if (structure is not null) resolvedFrom = "section";

            structure ??= candidates.FirstOrDefault(x => x.ClassId == enrollment.ClassId && x.SectionId == null);
            if (structure is not null && resolvedFrom == "none") resolvedFrom = "class";

            structure ??= candidates.FirstOrDefault(x => x.ClassGroupId != null && x.ClassId == null && x.SectionId == null);
            if (structure is not null && resolvedFrom == "none") resolvedFrom = "classgroup";
        }

        if (structure is null)
        {
            return Result.Success(new StudentAssignedStructureResponse
            {
                StudentId = request.StudentId,
                AcademicYearId = request.AcademicYearId,
                ResolvedFrom = "none",
            });
        }

        return Result.Success(new StudentAssignedStructureResponse
        {
            StudentId = request.StudentId,
            AcademicYearId = request.AcademicYearId,
            FeeStructureId = structure.Id,
            StructureName = structure.Name,
            ResolvedFrom = resolvedFrom,
            OverrideAmount = overrideAmount,
            Lines = structure.Lines.Where(l => l.DeletedAt == null)
                .Select(l => FeeStructureResponseFactory.ToLineResponse(l)).ToList(),
        });
    }
}
