using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using AcademicYearEntity = SchoolERP.Domain.Academic.Entities.AcademicYear;
using SchoolERP.Domain.Rbac.Entities;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Academic.Commands.AcademicYear.BulkPatchAcademicYear;

public class BulkPatchAcademicYearCommandHandler : IRequestHandler<BulkPatchAcademicYearCommand, Result<bool>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public BulkPatchAcademicYearCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentTenantService tenantService,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<bool>> Handle(BulkPatchAcademicYearCommand command, CancellationToken cancellationToken)
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
            request.Name.PatchIfProvided(value => ay.Name = value);
            if (request.StartDate.HasValue) ay.StartDate = request.StartDate.Value;
            if (request.EndDate.HasValue) ay.EndDate = request.EndDate.Value;
            if (request.IsCurrent.HasValue)
            {
                if (request.IsCurrent.Value)
                {
                    var outsideCurrent = await _dbContext.EnsureUniqueAsync<AcademicYearEntity>(
                        a => a.BranchId == branchId && a.IsCurrent && a.Id != ay.Id && !a.IsDeleted,
                        "Another current academic year already exists.",
                        cancellationToken);
                    if (outsideCurrent != null) return outsideCurrent;
                }
                ay.IsCurrent = request.IsCurrent.Value;
                ay.Status = ay.IsCurrent ? "active" : ay.Status;
            }
            if (!string.IsNullOrWhiteSpace(request.Status)) ay.Status = request.Status;

            ay.UpdatedAt = DateTime.UtcNow;
        }

        // Audit
        var audit = new RbacAuditLog
        {
            TenantId = tenantId,
            PerformedBy = _currentUserService.GetUserId() ?? 0,
            Resource = "AcademicYear",
            Action = "bulk_patch",
            NewValues = System.Text.Json.JsonSerializer.Serialize(new { request.Ids, request.Name, request.StartDate, request.EndDate, request.IsCurrent, request.Status }),
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Set<RbacAuditLog>().Add(audit);

        return Result.Success(true);
    }
}