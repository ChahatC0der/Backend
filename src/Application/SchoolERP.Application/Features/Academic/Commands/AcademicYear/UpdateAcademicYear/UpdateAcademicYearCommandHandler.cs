using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Academic.DTOs;
using AcademicYearEntity = SchoolERP.Domain.Academic.Entities.AcademicYear;
using SchoolERP.Domain.Rbac.Entities;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Academic.Commands.AcademicYear.UpdateAcademicYear;

public class UpdateAcademicYearCommandHandler : IRequestHandler<UpdateAcademicYearCommand, Result<AcademicYearResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public UpdateAcademicYearCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentTenantService tenantService,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<AcademicYearResponse>> Handle(UpdateAcademicYearCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var branchId = _tenantService.GetBranchId();
        var tenantId = _tenantService.GetTenantId();

        // Fetch using helper
        var entityResult = await _dbContext.GetEntityAsync<AcademicYearEntity>(
            ay => ay.Id == request.Id && ay.BranchId == branchId && !ay.IsDeleted,
            "AcademicYear",
            request.Id.ToString(),
            cancellationToken);
        if (entityResult.IsFailure) return entityResult.Error;

        var academicYear = entityResult.Value;

        // Uniqueness checks
        var nameConflict = await _dbContext.EnsureUniqueAsync<AcademicYearEntity>(
            ay => ay.BranchId == branchId && ay.Name == request.Name && ay.Id != request.Id && !ay.IsDeleted,
            $"Academic year '{request.Name}' already exists.",
            cancellationToken);
        if (nameConflict != null) return nameConflict;

        if (request.IsCurrent && !academicYear.IsCurrent)
        {
            var currentConflict = await _dbContext.EnsureUniqueAsync<AcademicYearEntity>(
                ay => ay.BranchId == branchId && ay.IsCurrent && ay.Id != request.Id && !ay.IsDeleted,
                "Another current academic year already exists.",
                cancellationToken);
            if (currentConflict != null) return currentConflict;
        }

        var oldValues = System.Text.Json.JsonSerializer.Serialize(new { academicYear.Name, academicYear.StartDate, academicYear.EndDate, academicYear.IsCurrent, academicYear.Status });

        academicYear.Name = request.Name;
        academicYear.StartDate = request.StartDate;
        academicYear.EndDate = request.EndDate;
        academicYear.IsCurrent = request.IsCurrent;
        academicYear.Status = request.Status;
        academicYear.UpdatedAt = DateTime.UtcNow;

        // Audit
        var audit = new RbacAuditLog
        {
            TenantId = tenantId,
            PerformedBy = _currentUserService.GetUserId() ?? 0,
            Resource = "AcademicYear",
            Action = "update",
            OldValues = oldValues,
            NewValues = System.Text.Json.JsonSerializer.Serialize(new { academicYear.Name, academicYear.StartDate, academicYear.EndDate, academicYear.IsCurrent, academicYear.Status }),
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Set<RbacAuditLog>().Add(audit);

        return Result.Success(academicYear.Adapt<AcademicYearResponse>());
    }
}