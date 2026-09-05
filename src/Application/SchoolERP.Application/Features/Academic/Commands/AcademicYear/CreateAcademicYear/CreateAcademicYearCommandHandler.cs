using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Academic.DTOs;
using AcademicYearEntity = SchoolERP.Domain.Academic.Entities.AcademicYear;
using SchoolERP.Domain.Rbac.Entities;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Academic.Commands.AcademicYear.CreateAcademicYear;

public class CreateAcademicYearCommandHandler : IRequestHandler<CreateAcademicYearCommand, Result<AcademicYearResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public CreateAcademicYearCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentTenantService tenantService,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<AcademicYearResponse>> Handle(CreateAcademicYearCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var branchId = _tenantService.GetBranchId();
        var tenantId = _tenantService.GetTenantId();

        // Uniqueness checks using helper
        var nameConflict = await _dbContext.EnsureUniqueAsync<AcademicYearEntity>(
            ay => ay.BranchId == branchId && ay.Name == request.Name && !ay.IsDeleted,
            $"Academic year '{request.Name}' already exists.",
            cancellationToken);
        if (nameConflict != null) return nameConflict;

        if (request.IsCurrent)
        {
            var currentConflict = await _dbContext.EnsureUniqueAsync<AcademicYearEntity>(
                ay => ay.BranchId == branchId && ay.IsCurrent && !ay.IsDeleted,
                "Another current academic year already exists.",
                cancellationToken);
            if (currentConflict != null) return currentConflict;
        }

        var academicYear = request.Adapt<AcademicYearEntity>();
        academicYear.Status = request.IsCurrent ? "active" : "upcoming";

        _dbContext.Set<AcademicYearEntity>().Add(academicYear);

        // Audit
        var audit = new RbacAuditLog
        {
            TenantId = tenantId,
            PerformedBy = _currentUserService.GetUserId() ?? 0,
            Resource = "AcademicYear",
            Action = "create",
            NewValues = System.Text.Json.JsonSerializer.Serialize(new { request.Name, request.StartDate, request.EndDate, request.IsCurrent }),
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Set<RbacAuditLog>().Add(audit);

        return Result.Success(academicYear.Adapt<AcademicYearResponse>());
    }
}