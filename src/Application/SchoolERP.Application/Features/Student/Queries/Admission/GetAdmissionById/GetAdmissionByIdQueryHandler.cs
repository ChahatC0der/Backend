using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Student.DTOs;
using SchoolERP.Domain.Shared.Results;
using AcademicYearEntity = SchoolERP.Domain.Academic.Entities.AcademicYear;
using ClassEntity = SchoolERP.Domain.Academic.Entities.Class;
using SectionEntity = SchoolERP.Domain.Academic.Entities.Section;
using RegistrationEntity = SchoolERP.Domain.Student.Registration.Entities.Registration;
using StudentEntity = SchoolERP.Domain.Student.Entities.Student;
using UserEntity = SchoolERP.Domain.Rbac.Entities.User;
using AdmissionEntity = SchoolERP.Domain.Student.Admission.Entities.Admission;

namespace SchoolERP.Application.Features.Student.Queries.Admission.GetAdmissionById;

public class GetAdmissionByIdQueryHandler : IRequestHandler<GetAdmissionByIdQuery, Result<AdmissionResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;

    public GetAdmissionByIdQueryHandler(IApplicationDbContext dbContext, ICurrentBranchService branchService)
    {
        _dbContext = dbContext;
        _branchService = branchService;
    }

    public async Task<Result<AdmissionResponse>> Handle(GetAdmissionByIdQuery query, CancellationToken cancellationToken)
    {
        var branchId = _branchService.GetBranchId();
        if (branchId == null) return Error.Conflict("Please Choose a Branch");

        var admission = await _dbContext.Set<AdmissionEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == query.Id && a.BranchId == branchId && !a.IsDeleted, cancellationToken);

        if (admission == null)
            return Error.NotFound("Admission", query.Id.ToString());

        var response = admission.Adapt<AdmissionResponse>();

        var ayName = await _dbContext.Set<AcademicYearEntity>().AsNoTracking()
            .Where(a => a.Id == admission.AcademicYearId).Select(a => a.Name).FirstOrDefaultAsync(cancellationToken);
        var className = await _dbContext.Set<ClassEntity>().AsNoTracking()
            .Where(c => c.Id == admission.ClassId).Select(c => c.Name).FirstOrDefaultAsync(cancellationToken);
        var sectionName = await _dbContext.Set<SectionEntity>().AsNoTracking()
            .Where(s => s.Id == admission.SectionId).Select(s => s.Name).FirstOrDefaultAsync(cancellationToken);

        string? regNumber = null;
        if (admission.RegistrationId.HasValue)
            regNumber = await _dbContext.Set<RegistrationEntity>().AsNoTracking()
                .Where(r => r.Id == admission.RegistrationId.Value).Select(r => r.RegistrationNumber).FirstOrDefaultAsync(cancellationToken);

        string? studentName = null;
        string? enrollmentId = null;
        if (admission.StudentId.HasValue)
        {
            var studentInfo = await _dbContext.Set<StudentEntity>().AsNoTracking()
                .Where(s => s.Id == admission.StudentId.Value)
                .Select(s => new { s.EnrollmentId, FullName = (s.FirstName + " " + (s.LastName ?? "")).Trim() })
                .FirstOrDefaultAsync(cancellationToken);
            if (studentInfo != null)
            {
                studentName = studentInfo.FullName;
                enrollmentId = studentInfo.EnrollmentId;
            }
        }

        string? enrolledByName = null;
        if (admission.EnrolledBy.HasValue)
            enrolledByName = await _dbContext.Set<UserEntity>().AsNoTracking()
                .Where(u => u.Id == admission.EnrolledBy.Value).Select(u => u.Name).FirstOrDefaultAsync(cancellationToken);

        return Result.Success(response with
        {
            AcademicYearName = ayName,
            ClassName = className,
            SectionName = sectionName,
            RegistrationNumber = regNumber,
            StudentName = studentName,
            EnrollmentId = enrollmentId,
            EnrolledByName = enrolledByName
        });
    }
}
