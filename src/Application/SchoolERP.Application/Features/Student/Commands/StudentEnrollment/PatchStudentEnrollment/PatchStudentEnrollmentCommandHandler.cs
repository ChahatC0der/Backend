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

namespace SchoolERP.Application.Features.Student.Commands.StudentEnrollment.PatchStudentEnrollment;

public class PatchStudentEnrollmentCommandHandler : IRequestHandler<PatchStudentEnrollmentCommand, Result<StudentEnrollmentResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;

    public PatchStudentEnrollmentCommandHandler(IApplicationDbContext dbContext, ICurrentBranchService branchService)
    {
        _dbContext = dbContext;
        _branchService = branchService;
    }

    public async Task<Result<StudentEnrollmentResponse>> Handle(PatchStudentEnrollmentCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var branchId = _branchService.GetBranchId() ?? Guid.Empty;

        var entityResult = await _dbContext.GetEntityAsync<StudentEnrollmentEntity>(
            e => e.Id == request.Id && !e.IsDeleted,
            "StudentEnrollment", request.Id.ToString(), cancellationToken);
        if (entityResult.IsFailure) return entityResult.Error;

        var enrollment = entityResult.Value;

        if (request.AcademicYearId.HasValue)
        {
            var ay = await _dbContext.Set<AcademicYearEntity>()
                .FirstOrDefaultAsync(a => a.Id == request.AcademicYearId.Value && a.BranchId == branchId && !a.IsDeleted, cancellationToken);
            if (ay == null) return Error.NotFound("AcademicYear", request.AcademicYearId.Value.ToString());
            enrollment.AcademicYearId = request.AcademicYearId.Value;
        }

        if (request.ClassId.HasValue)
        {
            var cls = await _dbContext.Set<ClassEntity>()
                .FirstOrDefaultAsync(c => c.Id == request.ClassId.Value && c.BranchId == branchId && !c.IsDeleted, cancellationToken);
            if (cls == null) return Error.NotFound("Class", request.ClassId.Value.ToString());
            enrollment.ClassId = request.ClassId.Value;
        }

        if (request.SectionId.HasValue)
        {
            var sec = await _dbContext.Set<SectionEntity>()
                .FirstOrDefaultAsync(s => s.Id == request.SectionId.Value && s.ClassId == enrollment.ClassId && !s.IsDeleted, cancellationToken);
            if (sec == null) return Error.NotFound("Section", request.SectionId.Value.ToString());
            enrollment.SectionId = request.SectionId.Value;
        }

        request.RollNumber.PatchIfProvided(v => enrollment.RollNumber = v);
        if (request.IsCurrent.HasValue) enrollment.IsCurrent = request.IsCurrent.Value;
        if (request.EnrolledAt.HasValue) enrollment.EnrolledAt = request.EnrolledAt.Value;
        if (request.LeftAt.HasValue) enrollment.LeftAt = request.LeftAt.Value;

        enrollment.UpdatedAt = DateTime.UtcNow;

        var response = enrollment.Adapt<StudentEnrollmentResponse>();
        return Result.Success(response);
    }
}
