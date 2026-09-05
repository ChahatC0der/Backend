using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using ClassEntity = SchoolERP.Domain.Academic.Entities.Class;
using SectionEntity = SchoolERP.Domain.Academic.Entities.Section;
using SchoolERP.Domain.Rbac.Entities;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Academic.Commands.Section.BulkUpdateSection;

public class BulkUpdateSectionCommandHandler : IRequestHandler<BulkUpdateSectionCommand, Result<bool>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public BulkUpdateSectionCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentTenantService tenantService,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<bool>> Handle(BulkUpdateSectionCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var branchId = _tenantService.GetBranchId();
        var tenantId = _tenantService.GetTenantId();

        var classExists = await _dbContext.EnsureEntityExistsAsync<ClassEntity>(request.ClassId, cancellationToken);
        if (classExists != null) return classExists;

        var entities = await _dbContext.Set<SectionEntity>()
            .Where(s => s.BranchId == branchId && request.Ids.Contains(s.Id) && !s.IsDeleted)
            .ToListAsync(cancellationToken);

        if (entities.Count != request.Ids.Count)
            return Error.NotFound("Sections", request.Ids.ToString());

        // Ensure no other section in same class has same name (outside selected)
        var outsideDuplicate = await _dbContext.EnsureUniqueAsync<SectionEntity>(
            s => s.ClassId == request.ClassId && s.Name == request.Name && !request.Ids.Contains(s.Id) && !s.IsDeleted,
            $"Another section with name '{request.Name}' already exists in this class.",
            cancellationToken);
        if (outsideDuplicate != null) return outsideDuplicate;

        foreach (var s in entities)
        {
            s.ClassId = request.ClassId;
            s.Name = request.Name;
            s.Capacity = request.Capacity;
            s.UpdatedAt = DateTime.UtcNow;
        }

        var audit = new RbacAuditLog
        {
            TenantId = tenantId,
            PerformedBy = _currentUserService.GetUserId() ?? 0,
            Resource = "Section",
            Action = "bulk_update",
            NewValues = System.Text.Json.JsonSerializer.Serialize(new { request.Ids, request.ClassId, request.Name, request.Capacity }),
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Set<RbacAuditLog>().Add(audit);

        return Result.Success(true);
    }
}