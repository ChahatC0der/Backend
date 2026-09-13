using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Student.DTOs;
using SchoolERP.Domain.Shared.Results;
using StudentEntity = SchoolERP.Domain.Student.Entities.Student;
using StudentEnrollmentEntity = SchoolERP.Domain.Student.Entities.StudentEnrollment;
using AcademicYearEntity = SchoolERP.Domain.Academic.Entities.AcademicYear;
using ClassEntity = SchoolERP.Domain.Academic.Entities.Class;
using SectionEntity = SchoolERP.Domain.Academic.Entities.Section;

namespace SchoolERP.Application.Features.Student.Queries.StudentEnrollment.GetStudentEnrollmentById;

public class GetStudentEnrollmentByIdQueryHandler : IRequestHandler<GetStudentEnrollmentByIdQuery, Result<StudentEnrollmentResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;

    public GetStudentEnrollmentByIdQueryHandler(IApplicationDbContext dbContext, ICurrentBranchService branchService)
    {
        _dbContext = dbContext;
        _branchService = branchService;
    }

    public async Task<Result<StudentEnrollmentResponse>> Handle(GetStudentEnrollmentByIdQuery query, CancellationToken cancellationToken)
    {
        var branchId = _branchService.GetBranchId() ?? Guid.Empty;

        var enrollment = await (
            from e in _dbContext.Set<StudentEnrollmentEntity>()
            join s in _dbContext.Set<StudentEntity>() on e.StudentId equals s.Id
            where e.Id == query.Id && !e.IsDeleted && !s.IsDeleted && s.BranchId == branchId
            select e
        ).AsNoTracking().FirstOrDefaultAsync(cancellationToken);

        if (enrollment == null)
            return Error.NotFound("StudentEnrollment", query.Id.ToString());

        var student = await _dbContext.Set<StudentEntity>()
            .AsNoTracking().FirstOrDefaultAsync(s => s.Id == enrollment.StudentId, cancellationToken);

        var ay = await _dbContext.Set<AcademicYearEntity>()
            .AsNoTracking().FirstOrDefaultAsync(a => a.Id == enrollment.AcademicYearId, cancellationToken);

        var cls = await _dbContext.Set<ClassEntity>()
            .AsNoTracking().FirstOrDefaultAsync(c => c.Id == enrollment.ClassId, cancellationToken);

        var sec = await _dbContext.Set<SectionEntity>()
            .AsNoTracking().FirstOrDefaultAsync(s => s.Id == enrollment.SectionId, cancellationToken);

        var response = new StudentEnrollmentResponse(
            enrollment.Id,
            enrollment.StudentId,
            student != null ? (student.FirstName + " " + (student.LastName ?? "")).Trim() : string.Empty,
            enrollment.AcademicYearId,
            ay?.Name,
            enrollment.ClassId,
            cls?.Name,
            enrollment.SectionId,
            sec?.Name,
            enrollment.RollNumber,
            enrollment.IsCurrent,
            enrollment.EnrolledAt,
            enrollment.LeftAt,
            enrollment.CreatedAt
        );

        return Result.Success(response);
    }
}
