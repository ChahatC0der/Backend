using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Master.DTOs;
using MasterCategoryEntity = SchoolERP.Domain.Master.Entities.MasterCategory;
using SchoolERP.Domain.Rbac.Entities;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Master.Commands.MasterCategory.CreateMasterCategory;

public class CreateMasterCategoryCommandHandler : IRequestHandler<CreateMasterCategoryCommand, Result<MasterCategoryResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public CreateMasterCategoryCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentTenantService tenantService,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<MasterCategoryResponse>> Handle(CreateMasterCategoryCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var tenantId = _tenantService.GetTenantId();

        // Check module exists
        var moduleExists = await _dbContext.EnsureEntityExistsAsync<Module>(request.ModuleId, cancellationToken);
        if (moduleExists != null) return moduleExists;

        // Uniqueness check (ModuleId, TenantId, Key)
        var conflict = await _dbContext.EnsureUniqueAsync<MasterCategoryEntity>(
            mc => mc.ModuleId == request.ModuleId && mc.TenantId == tenantId && mc.Key == request.Key && !mc.IsDeleted,
            $"Master category with key '{request.Key}' already exists.",
            cancellationToken);
        if (conflict != null) return conflict;

        var category = request.Adapt<MasterCategoryEntity>();
        category.TenantId = tenantId;   // tenant-specific

        _dbContext.Set<MasterCategoryEntity>().Add(category);

        // Audit
        var audit = new RbacAuditLog
        {
            TenantId = tenantId,
            PerformedBy = _currentUserService.GetUserId() ?? 0,
            Resource = "MasterCategory",
            Action = "create",
            NewValues = System.Text.Json.JsonSerializer.Serialize(new { request.ModuleId, request.Key, request.Name, request.Description, request.IsSystem, request.IsActive }),
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Set<RbacAuditLog>().Add(audit);

        return Result.Success(category.Adapt<MasterCategoryResponse>());
    }
}