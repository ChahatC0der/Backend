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

namespace SchoolERP.Application.Features.Student.Commands.StudentEnrollment.CreateStudentEnrollment;

public class CreateStudentEnrollmentCommandHandler : IRequestHandler<CreateStudentEnrollmentCommand, Result<StudentEnrollmentResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;

    public CreateStudentEnrollmentCommandHandler(IApplicationDbContext dbContext, ICurrentBranchService branchService)
    {
        _dbContext = dbContext;
        _branchService = branchService;
    }

    public async Task<Result<StudentEnrollmentResponse>> Handle(CreateStudentEnrollmentCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var branchId = _branchService.GetBranchId() ?? Guid.Empty;

        // Verify student
        var student = await _dbContext.Set<StudentEntity>()
            .FirstOrDefaultAsync(s => s.Id == request.StudentId && s.BranchId == branchId && !s.IsDeleted, cancellationToken);
        if (student == null) return Error.NotFound("Student", request.StudentId.ToString());

        // Verify academic year
        var academicYear = await _dbContext.Set<AcademicYearEntity>()
            .FirstOrDefaultAsync(a => a.Id == request.AcademicYearId && a.BranchId == branchId && !a.IsDeleted, cancellationToken);
        if (academicYear == null) return Error.NotFound("AcademicYear", request.AcademicYearId.ToString());

        // Verify class
        var classEntity = await _dbContext.Set<ClassEntity>()
            .FirstOrDefaultAsync(c => c.Id == request.ClassId && c.BranchId == branchId && !c.IsDeleted, cancellationToken);
        if (classEntity == null) return Error.NotFound("Class", request.ClassId.ToString());

        // Verify section
        var section = await _dbContext.Set<SectionEntity>()
            .FirstOrDefaultAsync(s => s.Id == request.SectionId && s.ClassId == request.ClassId && !s.IsDeleted, cancellationToken);
        if (section == null) return Error.NotFound("Section", request.SectionId.ToString());

        // Uniqueness: one enrollment per student per academic year
        var conflict = await _dbContext.EnsureUniqueAsync<StudentEnrollmentEntity>(
            e => e.StudentId == request.StudentId && e.AcademicYearId == request.AcademicYearId && !e.IsDeleted,
            "Student already has an enrollment for this academic year.",
            cancellationToken);
        if (conflict != null) return conflict;

        var enrollment = request.Adapt<StudentEnrollmentEntity>();
        enrollment.IsCurrent = true;
        if (enrollment.EnrolledAt == default) enrollment.EnrolledAt = DateOnly.FromDateTime(DateTime.UtcNow);

        _dbContext.Set<StudentEnrollmentEntity>().Add(enrollment);

        // Prepare response
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
