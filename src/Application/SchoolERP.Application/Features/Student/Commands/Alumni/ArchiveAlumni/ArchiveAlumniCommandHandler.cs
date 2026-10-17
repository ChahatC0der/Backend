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
using AlumniEntity = SchoolERP.Domain.Student.Alumni.Entities.Alumni;

namespace SchoolERP.Application.Features.Student.Commands.Alumni.ArchiveAlumni;

public class ArchiveAlumniCommandHandler : IRequestHandler<ArchiveAlumniCommand, Result<AlumniResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public ArchiveAlumniCommandHandler(
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

    public async Task<Result<AlumniResponse>> Handle(ArchiveAlumniCommand command, CancellationToken cancellationToken)
    {
        var branchId = _branchService.GetBranchId();
        var tenantId = _tenantService.GetTenantId();

        if (branchId == null) return Error.Conflict("Please Choose a Branch");

        var entityResult = await _dbContext.GetEntityAsync<AlumniEntity>(
            a => a.Id == command.Id && a.BranchId == branchId && !a.IsDeleted,
            "Alumni", command.Id.ToString(), cancellationToken);
        if (entityResult.IsFailure) return entityResult.Error;

        var alumni = entityResult.Value;

        alumni.IsArchived = true;
        if (command.IsDeceased)
        {
            alumni.IsDeceased = true;
            alumni.CurrentStatus = "deceased";
        }
        alumni.UpdatedAt = DateTime.UtcNow;

        var userId = _currentUserService.GetUserId() ?? 0;
        //_dbContext.Set<RbacAuditLog>().Add(new RbacAuditLog
        //{
        //    TenantId = tenantId,
        //    PerformedBy = userId,
        //    Resource = "Alumni",
        //    Action = "archive",
        //    NewValues = System.Text.Json.JsonSerializer.Serialize(new { IsArchived = true, command.IsDeceased }),
        //    CreatedAt = DateTime.UtcNow
        //});

        var response = alumni.Adapt<AlumniResponse>();
        return Result.Success(await FillComputedAsync(response, cancellationToken));
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
