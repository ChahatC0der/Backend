using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Student.DTOs;
using SchoolERP.Domain.Shared.Results;
using ClassEntity = SchoolERP.Domain.Academic.Entities.Class;
using SectionEntity = SchoolERP.Domain.Academic.Entities.Section;
using FamilyEntity = SchoolERP.Domain.Student.Entities.Family;
using StudentEntity = SchoolERP.Domain.Student.Entities.Student;
using StudentEnrollmentEntity = SchoolERP.Domain.Student.Entities.StudentEnrollment;

namespace SchoolERP.Application.Features.Student.Queries.Family.GetFamilyDetailed;

public class GetFamilyDetailedQueryHandler : IRequestHandler<GetFamilyDetailedQuery, Result<FamilyDetailedResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;

    public GetFamilyDetailedQueryHandler(IApplicationDbContext dbContext, ICurrentBranchService branchService)
    {
        _dbContext = dbContext;
        _branchService = branchService;
    }

    public async Task<Result<FamilyDetailedResponse>> Handle(GetFamilyDetailedQuery query, CancellationToken cancellationToken)
    {
        var branchId = _branchService.GetBranchId();
        if (branchId == null) return Error.Conflict("Please Choose a Branch");

        var family = await _dbContext.Set<FamilyEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == query.Id && f.BranchId == branchId && !f.IsDeleted, cancellationToken);
        if (family == null) return Error.NotFound("Family", query.Id.ToString());

        var students = await _dbContext.Set<StudentEntity>()
            .AsNoTracking()
            .Where(s => s.FamilyId == family.Id && !s.IsDeleted)
            .ToListAsync(cancellationToken);

        var studentIds = students.Select(s => s.Id).ToList();

        var enrollments = await _dbContext.Set<StudentEnrollmentEntity>()
            .AsNoTracking()
            .Where(e => studentIds.Contains(e.StudentId) && e.IsCurrent && !e.IsDeleted)
            .ToListAsync(cancellationToken);

        var classIds = enrollments.Select(e => e.ClassId).Distinct().ToList();
        var sectionIds = enrollments.Select(e => e.SectionId).Distinct().ToList();

        var classNames = await _dbContext.Set<ClassEntity>().AsNoTracking()
            .Where(c => classIds.Contains(c.Id)).ToDictionaryAsync(c => c.Id, c => c.Name, cancellationToken);
        var sectionNames = await _dbContext.Set<SectionEntity>().AsNoTracking()
            .Where(s => sectionIds.Contains(s.Id)).ToDictionaryAsync(s => s.Id, s => s.Name, cancellationToken);

        var enrollmentByStudent = enrollments.GroupBy(e => e.StudentId).ToDictionary(g => g.Key, g => g.First());

        var studentList = students.Select(s =>
        {
            string? className = null;
            string? sectionName = null;

            if (enrollmentByStudent.TryGetValue(s.Id, out var en))
            {
                if (classNames.TryGetValue(en.ClassId, out var cn)) className = cn;
                if (sectionNames.TryGetValue(en.SectionId, out var sn)) sectionName = sn;
            }

            return new FamilyStudentLightResponse(
                s.Id,
                s.EnrollmentId,
                (s.FirstName + " " + (s.LastName ?? "")).Trim(),
                className,
                sectionName,
                s.Status);
        }).ToList();

        return Result.Success(new FamilyDetailedResponse(
            family.Id,
            family.TenantId,
            family.BranchId,
            family.HeadName,
            family.HeadMobile,
            family.HeadEmail,
            studentList.Count,
            studentList,
            family.CreatedAt,
            family.UpdatedAt));
    }
}
