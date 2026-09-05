using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using AcademicYearEntity = SchoolERP.Domain.Academic.Entities.AcademicYear;
using SchoolERP.Domain.Rbac.Entities;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Academic.Commands.AcademicYear.DeleteAcademicYear;

public class DeleteAcademicYearCommandHandler : IRequestHandler<DeleteAcademicYearCommand, Result<bool>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public DeleteAcademicYearCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentTenantService tenantService,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<bool>> Handle(DeleteAcademicYearCommand command, CancellationToken cancellationToken)
    {
        var branchId = _tenantService.GetBranchId();
        var tenantId = _tenantService.GetTenantId();

        var entityResult = await _dbContext.GetEntityAsync<AcademicYearEntity>(
            ay => ay.Id == command.Id && ay.BranchId == branchId && !ay.IsDeleted,
            "AcademicYear",
            command.Id.ToString(),
            cancellationToken);
        if (entityResult.IsFailure) return entityResult.Error;

        var academicYear = entityResult.Value;
        academicYear.IsDeleted = true;
        academicYear.DeletedAt = DateTime.UtcNow;
        academicYear.UpdatedAt = DateTime.UtcNow;

        // Audit
        var audit = new RbacAuditLog
        {
            TenantId = tenantId,
            PerformedBy = _currentUserService.GetUserId() ?? 0,
            Resource = "AcademicYear",
            Action = "delete",
            OldValues = System.Text.Json.JsonSerializer.Serialize(new { academicYear.Name, academicYear.StartDate, academicYear.EndDate }),
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Set<RbacAuditLog>().Add(audit);

        return Result.Success(true);
    }
}