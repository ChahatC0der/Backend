using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using AcademicYearEntity = SchoolERP.Domain.Academic.Entities.AcademicYear;
using SchoolERP.Domain.Rbac.Entities;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Academic.Commands.AcademicYear.BulkUpdateAcademicYear;

public class BulkUpdateAcademicYearCommandHandler : IRequestHandler<BulkUpdateAcademicYearCommand, Result<bool>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public BulkUpdateAcademicYearCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentTenantService tenantService,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<bool>> Handle(BulkUpdateAcademicYearCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var branchId = _tenantService.GetBranchId();
        var tenantId = _tenantService.GetTenantId();

        if (request.IsCurrent)
        {
            var outsideCurrent = await _dbContext.EnsureUniqueAsync<AcademicYearEntity>(
                ay => ay.BranchId == branchId && ay.IsCurrent && !request.Ids.Contains(ay.Id) && !ay.IsDeleted,
                "Another current academic year already exists outside selected IDs.",
                cancellationToken);
            if (outsideCurrent != null) return outsideCurrent;

            if (request.Ids.Count > 1)
                return Error.Conflict("Only one academic year can be current.");
        }

        var entities = await _dbContext.Set<AcademicYearEntity>()
            .Where(ay => ay.BranchId == branchId && request.Ids.Contains(ay.Id) && !ay.IsDeleted)
            .ToListAsync(cancellationToken);

        if (entities.Count != request.Ids.Count)
            return Error.NotFound("AcademicYear", request.Ids.ToString());

        foreach (var ay in entities)
        {
            ay.Name = request.Name;
            ay.StartDate = request.StartDate;
            ay.EndDate = request.EndDate;
            ay.IsCurrent = request.IsCurrent;
            ay.Status = request.IsCurrent ? "active" : request.Status;
            ay.UpdatedAt = DateTime.UtcNow;
        }

        // Audit
        var audit = new RbacAuditLog
        {
            TenantId = tenantId,
            PerformedBy = _currentUserService.GetUserId() ?? 0,
            Resource = "AcademicYear",
            Action = "bulk_update",
            NewValues = System.Text.Json.JsonSerializer.Serialize(new { request.Ids, request.Name, request.StartDate, request.EndDate, request.IsCurrent, request.Status }),
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Set<RbacAuditLog>().Add(audit);

        return Result.Success(true);
    }
}