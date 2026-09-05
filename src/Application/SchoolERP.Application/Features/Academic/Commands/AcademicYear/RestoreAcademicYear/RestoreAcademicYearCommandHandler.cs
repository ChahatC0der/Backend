using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using AcademicYearEntity = SchoolERP.Domain.Academic.Entities.AcademicYear;
using SchoolERP.Domain.Rbac.Entities;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Academic.Commands.AcademicYear.RestoreAcademicYear;

public class RestoreAcademicYearCommandHandler : IRequestHandler<RestoreAcademicYearCommand, Result<bool>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public RestoreAcademicYearCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentTenantService tenantService,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<bool>> Handle(RestoreAcademicYearCommand command, CancellationToken cancellationToken)
    {
        var branchId = _tenantService.GetBranchId();
        var tenantId = _tenantService.GetTenantId();

        var academicYear = await _dbContext.Set<AcademicYearEntity>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(ay => ay.Id == command.Id && ay.BranchId == branchId && ay.IsDeleted, cancellationToken);

        if (academicYear == null)
            return Error.NotFound("AcademicYear", command.Id.ToString());

        academicYear.IsDeleted = false;
        academicYear.DeletedAt = null;
        academicYear.UpdatedAt = DateTime.UtcNow;

        // Audit
        var audit = new RbacAuditLog
        {
            TenantId = tenantId,
            PerformedBy = _currentUserService.GetUserId() ?? 0,
            Resource = "AcademicYear",
            Action = "restore",
            NewValues = System.Text.Json.JsonSerializer.Serialize(new { academicYear.Id, academicYear.Name }),
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Set<RbacAuditLog>().Add(audit);

        return Result.Success(true);
    }
}