using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using AcademicYearEntity = SchoolERP.Domain.Academic.Entities.AcademicYear;
using SchoolERP.Domain.Rbac.Entities;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Academic.Commands.AcademicYear.BulkDeleteAcademicYear;

public class BulkDeleteAcademicYearCommandHandler : IRequestHandler<BulkDeleteAcademicYearCommand, Result<bool>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public BulkDeleteAcademicYearCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentTenantService tenantService,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<bool>> Handle(BulkDeleteAcademicYearCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var branchId = _tenantService.GetBranchId();
        var tenantId = _tenantService.GetTenantId();

        var entities = await _dbContext.Set<AcademicYearEntity>()
            .Where(ay => ay.BranchId == branchId && request.Ids.Contains(ay.Id) && !ay.IsDeleted)
            .ToListAsync(cancellationToken);

        if (entities.Count != request.Ids.Count)
            return Error.NotFound("AcademicYear", request.Ids.ToString());

        foreach (var ay in entities)
        {
            ay.IsDeleted = true;
            ay.DeletedAt = DateTime.UtcNow;
            ay.UpdatedAt = DateTime.UtcNow;
        }

        // Audit
        var audit = new RbacAuditLog
        {
            TenantId = tenantId,
            PerformedBy = _currentUserService.GetUserId() ?? 0,
            Resource = "AcademicYear",
            Action = "bulk_delete",
            OldValues = System.Text.Json.JsonSerializer.Serialize(entities.Select(e => new { e.Id, e.Name })),
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Set<RbacAuditLog>().Add(audit);

        return Result.Success(true);
    }
}