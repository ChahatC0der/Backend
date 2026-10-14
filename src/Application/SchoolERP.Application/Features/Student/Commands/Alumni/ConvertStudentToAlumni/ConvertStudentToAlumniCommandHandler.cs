using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Student.DTOs;
using SchoolERP.Domain.Rbac.Entities;
using SchoolERP.Domain.Shared.Results;
using AcademicYearEntity = SchoolERP.Domain.Academic.Entities.AcademicYear;
using ClassEntity = SchoolERP.Domain.Academic.Entities.Class;
using StudentEntity = SchoolERP.Domain.Student.Entities.Student;
using AlumniEntity = SchoolERP.Domain.Student.Alumni.Entities.Alumni;

namespace SchoolERP.Application.Features.Student.Commands.Alumni.ConvertStudentToAlumni;

public class ConvertStudentToAlumniCommandHandler : IRequestHandler<ConvertStudentToAlumniCommand, Result<AlumniResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public ConvertStudentToAlumniCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentBranchService branchService,
        ICurrentTenantService tenantService,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _branchService = branchService;
        _tenantService = tenantService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<AlumniResponse>> Handle(ConvertStudentToAlumniCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var branchId = _branchService.GetBranchId();
        var tenantId = _tenantService.GetTenantId();

        if (branchId == null) return Error.Conflict("Please Choose a Branch");

        var student = await _dbContext.Set<StudentEntity>()
            .FirstOrDefaultAsync(s => s.Id == request.StudentId && s.BranchId == branchId && !s.IsDeleted, cancellationToken);
        if (student == null) return Error.NotFound("Student", request.StudentId.ToString());

        var exists = await _dbContext.Set<AlumniEntity>()
            .AnyAsync(a => a.StudentId == request.StudentId && !a.IsDeleted, cancellationToken);
        if (exists) return Error.Conflict("Alumni record already exists for this student.");

        var classErr = await _dbContext.EnsureEntityExistsAsync<ClassEntity>(request.LastClassId, cancellationToken);
        if (classErr != null) return Error.NotFound("Class", request.LastClassId.ToString());

        var ayErr = await _dbContext.EnsureEntityExistsAsync<AcademicYearEntity>(request.PassingAcademicYearId, cancellationToken);
        if (ayErr != null) return Error.NotFound("AcademicYear", request.PassingAcademicYearId.ToString());

        var alumni = new AlumniEntity
        {
            TenantId = tenantId,
            BranchId = branchId.Value,
            StudentId = student.Id,
            AlumniId = await GenerateAlumniIdAsync(branchId.Value, cancellationToken),
            FirstName = student.FirstName,
            LastName = student.LastName,
            DateOfBirth = student.DateOfBirth,
            Gender = student.Gender,
            GraduationDate = request.GraduationDate,
            LastClassId = request.LastClassId,
            PassingAcademicYearId = request.PassingAcademicYearId,
            FinalPercentage = request.FinalPercentage,
            CurrentStatus = "not_employed",
            ContactEmail = student.PersonalEmail,
            ContactMobile = student.PersonalMobile,
            Address = student.Address,
            PhotoUrl = student.PhotoUrl
        };

        _dbContext.Set<AlumniEntity>().Add(alumni);

        // Mark student as graduated
        student.Status = "graduated";
        student.UpdatedAt = DateTime.UtcNow;

        var userId = _currentUserService.GetUserId() ?? 0;
        _dbContext.Set<RbacAuditLog>().Add(new RbacAuditLog
        {
            TenantId = tenantId,
            PerformedBy = userId,
            Resource = "Alumni",
            Action = "convert_from_student",
            NewValues = System.Text.Json.JsonSerializer.Serialize(new
            {
                alumni.AlumniId,
                StudentId = student.Id,
                request.GraduationDate
            }),
            CreatedAt = DateTime.UtcNow
        });

        var response = alumni.Adapt<AlumniResponse>();
        return Result.Success(await FillComputedAsync(response, cancellationToken));
    }

    private async Task<string> GenerateAlumniIdAsync(Guid branchId, CancellationToken ct)
    {
        var year = DateTime.UtcNow.Year;
        var count = await _dbContext.Set<AlumniEntity>()
            .IgnoreQueryFilters()
            .Where(a => a.BranchId == branchId && a.CreatedAt.Year == year)
            .CountAsync(ct);
        return $"ALUM-{year}-{(count + 1):D4}";
    }

    private async Task<AlumniResponse> FillComputedAsync(AlumniResponse r, CancellationToken ct)
    {
        var className = await _dbContext.Set<ClassEntity>().AsNoTracking()
            .Where(c => c.Id == r.LastClassId).Select(c => c.Name).FirstOrDefaultAsync(ct);
        var ayName = await _dbContext.Set<AcademicYearEntity>().AsNoTracking()
            .Where(a => a.Id == r.PassingAcademicYearId).Select(a => a.Name).FirstOrDefaultAsync(ct);
        return r with { LastClassName = className, PassingAcademicYearName = ayName };
    }
}
