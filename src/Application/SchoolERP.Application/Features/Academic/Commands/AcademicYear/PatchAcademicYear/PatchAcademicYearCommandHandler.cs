using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Academic.DTOs;
using AcademicYearEntity = SchoolERP.Domain.Academic.Entities.AcademicYear;
using SchoolERP.Domain.Rbac.Entities;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Academic.Commands.AcademicYear.PatchAcademicYear;

public class PatchAcademicYearCommandHandler : IRequestHandler<PatchAcademicYearCommand, Result<AcademicYearResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public PatchAcademicYearCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentTenantService tenantService,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<AcademicYearResponse>> Handle(PatchAcademicYearCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var branchId = _tenantService.GetBranchId();
        var tenantId = _tenantService.GetTenantId();

        var entityResult = await _dbContext.GetEntityAsync<AcademicYearEntity>(
            ay => ay.Id == request.Id && ay.BranchId == branchId && !ay.IsDeleted,
            "AcademicYear",
            request.Id.ToString(),
            cancellationToken);
        if (entityResult.IsFailure) return entityResult.Error;

        var academicYear = entityResult.Value;
        var oldValues = System.Text.Json.JsonSerializer.Serialize(new { academicYear.Name, academicYear.StartDate, academicYear.EndDate, academicYear.IsCurrent, academicYear.Status });

        // Use PatchIfProvided for string
        request.Name.PatchIfProvided(value => academicYear.Name = value);

        if (request.StartDate.HasValue) academicYear.StartDate = request.StartDate.Value;
        if (request.EndDate.HasValue) academicYear.EndDate = request.EndDate.Value;

        if (request.IsCurrent.HasValue)
        {
            if (request.IsCurrent.Value && !academicYear.IsCurrent)
            {
                var currentConflict = await _dbContext.EnsureUniqueAsync<AcademicYearEntity>(
                    ay => ay.BranchId == branchId && ay.IsCurrent && ay.Id != academicYear.Id && !ay.IsDeleted,
                    "Another current academic year already exists.",
                    cancellationToken);
                if (currentConflict != null) return currentConflict;
            }
            academicYear.IsCurrent = request.IsCurrent.Value;
            academicYear.Status = academicYear.IsCurrent ? "active" : academicYear.Status;
        }

        if (!string.IsNullOrWhiteSpace(request.Status)) academicYear.Status = request.Status;

        academicYear.UpdatedAt = DateTime.UtcNow;

        // Audit
        var audit = new RbacAuditLog
        {
            TenantId = tenantId,
            PerformedBy = _currentUserService.GetUserId() ?? 0,
            Resource = "AcademicYear",
            Action = "patch",
            OldValues = oldValues,
            NewValues = System.Text.Json.JsonSerializer.Serialize(new { academicYear.Name, academicYear.StartDate, academicYear.EndDate, academicYear.IsCurrent, academicYear.Status }),
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Set<RbacAuditLog>().Add(audit);

        return Result.Success(academicYear.Adapt<AcademicYearResponse>());
    }
}