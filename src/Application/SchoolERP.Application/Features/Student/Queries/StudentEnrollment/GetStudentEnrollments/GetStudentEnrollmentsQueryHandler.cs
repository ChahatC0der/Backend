using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Common.DTOs;
using SchoolERP.Application.Features.Student.DTOs;
using SchoolERP.Domain.Shared.Results;
using StudentEntity = SchoolERP.Domain.Student.Entities.Student;
using StudentEnrollmentEntity = SchoolERP.Domain.Student.Entities.StudentEnrollment;
using AcademicYearEntity = SchoolERP.Domain.Academic.Entities.AcademicYear;
using ClassEntity = SchoolERP.Domain.Academic.Entities.Class;
using SectionEntity = SchoolERP.Domain.Academic.Entities.Section;

namespace SchoolERP.Application.Features.Student.Queries.StudentEnrollment.GetStudentEnrollments;

public class GetStudentEnrollmentsQueryHandler : IRequestHandler<GetStudentEnrollmentsQuery, Result<PagedResponse<StudentEnrollmentResponse>>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;

    public GetStudentEnrollmentsQueryHandler(IApplicationDbContext dbContext, ICurrentBranchService branchService)
    {
        _dbContext = dbContext;
        _branchService = branchService;
    }

    public async Task<Result<PagedResponse<StudentEnrollmentResponse>>> Handle(GetStudentEnrollmentsQuery query, CancellationToken cancellationToken)
    {
        var branchId = _branchService.GetBranchId() ?? Guid.Empty;
        var request = query.Request;

        // Join with Student to enforce branch isolation
        var queryable = from e in _dbContext.Set<StudentEnrollmentEntity>()
                        join s in _dbContext.Set<StudentEntity>() on e.StudentId equals s.Id
                        where !e.IsDeleted && !s.IsDeleted && s.BranchId == branchId
                        select new { e, s };

        if (query.StudentId.HasValue)
            queryable = queryable.Where(x => x.e.StudentId == query.StudentId.Value);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var search = request.SearchTerm.ToLower();
            queryable = queryable.Where(x =>
                x.s.FirstName.ToLower().Contains(search) ||
                (x.s.LastName != null && x.s.LastName.ToLower().Contains(search)) ||
                (x.e.RollNumber != null && x.e.RollNumber.ToLower().Contains(search)));
        }

        queryable = request.SortBy?.ToLower() switch
        {
            "enrolledat" => request.SortOrder?.ToLower() == "desc" ? queryable.OrderByDescending(x => x.e.EnrolledAt) : queryable.OrderBy(x => x.e.EnrolledAt),
            _ => queryable.OrderByDescending(x => x.e.EnrolledAt)
        };

        var totalCount = await queryable.CountAsync(cancellationToken);

        var rawItems = await queryable
            .Skip((request.Page - 1) * request.Size)
            .Take(request.Size)
            .Select(x => x.e)
            .ToListAsync(cancellationToken);

        // Load lookups
        var ayIds = rawItems.Select(i => i.AcademicYearId).Distinct().ToList();
        var classIds = rawItems.Select(i => i.ClassId).Distinct().ToList();
        var sectionIds = rawItems.Select(i => i.SectionId).Distinct().ToList();
        var studentIds = rawItems.Select(i => i.StudentId).Distinct().ToList();

        var academicYears = await _dbContext.Set<AcademicYearEntity>()
            .AsNoTracking().Where(a => ayIds.Contains(a.Id))
            .ToDictionaryAsync(a => a.Id, a => a.Name, cancellationToken);

        var classes = await _dbContext.Set<ClassEntity>()
            .AsNoTracking().Where(c => classIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => c.Name, cancellationToken);

        var sections = await _dbContext.Set<SectionEntity>()
            .AsNoTracking().Where(s => sectionIds.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id, s => s.Name, cancellationToken);

        var students = await _dbContext.Set<StudentEntity>()
            .AsNoTracking().Where(s => studentIds.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id, s => (s.FirstName + " " + (s.LastName ?? "")).Trim(), cancellationToken);

        var data = rawItems.Select(e => new StudentEnrollmentResponse(
            e.Id,
            e.StudentId,
            students.TryGetValue(e.StudentId, out var sn) ? sn : string.Empty,
            e.AcademicYearId,
            academicYears.TryGetValue(e.AcademicYearId, out var ay) ? ay : null,
            e.ClassId,
            classes.TryGetValue(e.ClassId, out var cn) ? cn : null,
            e.SectionId,
            sections.TryGetValue(e.SectionId, out var sname) ? sname : null,
            e.RollNumber,
            e.IsCurrent,
            e.EnrolledAt,
            e.LeftAt,
            e.CreatedAt
        )).ToList();

        return Result.Success(new PagedResponse<StudentEnrollmentResponse>
        {
            Data = data,
            TotalCount = totalCount,
            Page = request.Page,
            Size = request.Size
        });
    }
}
