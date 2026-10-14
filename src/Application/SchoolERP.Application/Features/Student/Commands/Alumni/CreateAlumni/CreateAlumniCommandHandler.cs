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

namespace SchoolERP.Application.Features.Student.Commands.Alumni.CreateAlumni;

public class CreateAlumniCommandHandler : IRequestHandler<CreateAlumniCommand, Result<AlumniResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public CreateAlumniCommandHandler(
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

    public async Task<Result<AlumniResponse>> Handle(CreateAlumniCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var branchId = _branchService.GetBranchId();
        var tenantId = _tenantService.GetTenantId();

        if (branchId == null) return Error.Conflict("Please Choose a Branch");

        var classErr = await _dbContext.EnsureEntityExistsAsync<ClassEntity>(request.LastClassId, cancellationToken);
        if (classErr != null) return Error.NotFound("Class", request.LastClassId.ToString());

        var ayErr = await _dbContext.EnsureEntityExistsAsync<AcademicYearEntity>(request.PassingAcademicYearId, cancellationToken);
        if (ayErr != null) return Error.NotFound("AcademicYear", request.PassingAcademicYearId.ToString());

        if (request.StudentId.HasValue)
        {
            var stuErr = await _dbContext.EnsureEntityExistsAsync<StudentEntity>(request.StudentId.Value, cancellationToken);
            if (stuErr != null) return Error.NotFound("Student", request.StudentId.Value.ToString());
        }

        var alumni = request.Adapt<AlumniEntity>();
        alumni.TenantId = tenantId;
        alumni.AlumniId = await GenerateAlumniIdAsync(branchId.Value, cancellationToken);
        alumni.CurrentStatus = request.CurrentStatus;

        _dbContext.Set<AlumniEntity>().Add(alumni);

        var userId = _currentUserService.GetUserId() ?? 0;
        _dbContext.Set<RbacAuditLog>().Add(new RbacAuditLog
        {
            TenantId = tenantId,
            PerformedBy = userId,
            Resource = "Alumni",
            Action = "create",
            NewValues = System.Text.Json.JsonSerializer.Serialize(new
            {
                alumni.AlumniId,
                alumni.FirstName,
                alumni.GraduationDate
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
