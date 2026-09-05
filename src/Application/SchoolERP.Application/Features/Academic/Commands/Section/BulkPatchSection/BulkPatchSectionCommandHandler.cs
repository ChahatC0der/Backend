using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using ClassEntity = SchoolERP.Domain.Academic.Entities.Class;
using SectionEntity = SchoolERP.Domain.Academic.Entities.Section;
using SchoolERP.Domain.Rbac.Entities;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Academic.Commands.Section.BulkPatchSection;

public class BulkPatchSectionCommandHandler : IRequestHandler<BulkPatchSectionCommand, Result<bool>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public BulkPatchSectionCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentTenantService tenantService,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<bool>> Handle(BulkPatchSectionCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var branchId = _tenantService.GetBranchId();
        var tenantId = _tenantService.GetTenantId();

        if (request.ClassId.HasValue)
        {
            var classExists = await _dbContext.EnsureEntityExistsAsync<ClassEntity>(request.ClassId.Value, cancellationToken);
            if (classExists != null) return classExists;
        }

        var entities = await _dbContext.Set<SectionEntity>()
            .Where(s => s.BranchId == branchId && request.Ids.Contains(s.Id) && !s.IsDeleted)
            .ToListAsync(cancellationToken);

        if (entities.Count != request.Ids.Count)
            return Error.NotFound("Sections", request.Ids.ToString());

        foreach (var s in entities)
        {
            if (request.ClassId.HasValue) s.ClassId = request.ClassId.Value;
            request.Name.PatchIfProvided(value => s.Name = value);
            if (request.Capacity.HasValue) s.Capacity = request.Capacity.Value;

            // Check uniqueness if name or class changed
            if (!string.IsNullOrWhiteSpace(request.Name) || request.ClassId.HasValue)
            {
                var conflict = await _dbContext.EnsureUniqueAsync<SectionEntity>(
                    x => x.ClassId == s.ClassId && x.Name == s.Name && x.Id != s.Id && !x.IsDeleted,
                    $"Another section with name '{s.Name}' already exists in this class.",
                    cancellationToken);
                if (conflict != null) return conflict;
            }

            s.UpdatedAt = DateTime.UtcNow;
        }

        var audit = new RbacAuditLog
        {
            TenantId = tenantId,
            PerformedBy = _currentUserService.GetUserId() ?? 0,
            Resource = "Section",
            Action = "bulk_patch",
            NewValues = System.Text.Json.JsonSerializer.Serialize(new { request.Ids, request.ClassId, request.Name, request.Capacity }),
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Set<RbacAuditLog>().Add(audit);

        return Result.Success(true);
    }
}