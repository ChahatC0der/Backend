using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Student.DTOs;
using SchoolERP.Domain.Shared.Results;
using StudentEntity = SchoolERP.Domain.Student.Entities.Student;
using StudentEnrollmentEntity = SchoolERP.Domain.Student.Entities.StudentEnrollment;
using AcademicYearEntity = SchoolERP.Domain.Academic.Entities.AcademicYear;
using ClassEntity = SchoolERP.Domain.Academic.Entities.Class;
using SectionEntity = SchoolERP.Domain.Academic.Entities.Section;

namespace SchoolERP.Application.Features.Student.Commands.StudentEnrollment.UpdateStudentEnrollment;

public class UpdateStudentEnrollmentCommandHandler : IRequestHandler<UpdateStudentEnrollmentCommand, Result<StudentEnrollmentResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;

    public UpdateStudentEnrollmentCommandHandler(IApplicationDbContext dbContext, ICurrentBranchService branchService)
    {
        _dbContext = dbContext;
        _branchService = branchService;
    }

    public async Task<Result<StudentEnrollmentResponse>> Handle(UpdateStudentEnrollmentCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var branchId = _branchService.GetBranchId() ?? Guid.Empty;

        var entityResult = await _dbContext.GetEntityAsync<StudentEnrollmentEntity>(
            e => e.Id == request.Id && !e.IsDeleted,
            "StudentEnrollment", request.Id.ToString(), cancellationToken);
        if (entityResult.IsFailure) return entityResult.Error;

        var enrollment = entityResult.Value;

        // Verify relations belong to same branch
        var student = await _dbContext.Set<StudentEntity>()
            .FirstOrDefaultAsync(s => s.Id == enrollment.StudentId && s.BranchId == branchId && !s.IsDeleted, cancellationToken);
        if (student == null) return Error.NotFound("Student", enrollment.StudentId.ToString());

        var academicYear = await _dbContext.Set<AcademicYearEntity>()
            .FirstOrDefaultAsync(a => a.Id == request.AcademicYearId && a.BranchId == branchId && !a.IsDeleted, cancellationToken);
        if (academicYear == null) return Error.NotFound("AcademicYear", request.AcademicYearId.ToString());

        var classEntity = await _dbContext.Set<ClassEntity>()
            .FirstOrDefaultAsync(c => c.Id == request.ClassId && c.BranchId == branchId && !c.IsDeleted, cancellationToken);
        if (classEntity == null) return Error.NotFound("Class", request.ClassId.ToString());

        var section = await _dbContext.Set<SectionEntity>()
            .FirstOrDefaultAsync(s => s.Id == request.SectionId && s.ClassId == request.ClassId && !s.IsDeleted, cancellationToken);
        if (section == null) return Error.NotFound("Section", request.SectionId.ToString());

        enrollment.AcademicYearId = request.AcademicYearId;
        enrollment.ClassId = request.ClassId;
        enrollment.SectionId = request.SectionId;
        enrollment.RollNumber = request.RollNumber;
        enrollment.IsCurrent = request.IsCurrent;
        enrollment.EnrolledAt = request.EnrolledAt;
        enrollment.LeftAt = request.LeftAt;
        enrollment.UpdatedAt = DateTime.UtcNow;

        var response = enrollment.Adapt<StudentEnrollmentResponse>();
        response = response with
        {
            StudentName = (student.FirstName + " " + (student.LastName ?? "")).Trim(),
            AcademicYearName = academicYear.Name,
            ClassName = classEntity.Name,
            SectionName = section.Name
        };

        return Result.Success(response);
    }
}
