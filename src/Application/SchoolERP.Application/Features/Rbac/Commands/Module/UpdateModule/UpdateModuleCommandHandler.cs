using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Rbac.DTOs;
using SchoolERP.Domain.Rbac.Entities;
using SchoolERP.Domain.Shared.Results;
using ModuleEntity = SchoolERP.Domain.Rbac.Entities.Module;

namespace SchoolERP.Application.Features.Rbac.Commands.Module.UpdateModule;

public class UpdateModuleCommandHandler : IRequestHandler<UpdateModuleCommand, Result<ModuleResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public UpdateModuleCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentTenantService tenantService,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<ModuleResponse>> Handle(UpdateModuleCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var tenantId = _tenantService.GetTenantId();

        var module = await _dbContext.Set<ModuleEntity>()
            .FirstOrDefaultAsync(m => m.Id == request.Id && !m.IsDeleted && m.TenantId == tenantId, cancellationToken);
        if (module == null)
            return Error.NotFound("Module", request.Id.ToString());

        if (module.IsSystem)
            return Error.Conflict("System modules cannot be updated.");

        if (request.ParentId.HasValue)
        {
            var parentExists = await _dbContext.EnsureEntityExistsAsync<ModuleEntity>(request.ParentId.Value, cancellationToken);
            if (parentExists != null) return Error.NotFound("Module", request.ParentId.Value.ToString());
        }

        var conflict = await _dbContext.EnsureUniqueAsync<ModuleEntity>(
            m => m.TenantId == tenantId && m.Key == request.Key && m.Id != request.Id && !m.IsDeleted,
            $"Module with key '{request.Key}' already exists.",
            cancellationToken);
        if (conflict != null) return conflict;

        var oldValues = System.Text.Json.JsonSerializer.Serialize(new { module.Name, module.Key, module.SortOrder, module.ParentId });

        module.Name = request.Name;
        module.Key = request.Key.Trim().ToLowerInvariant();
        module.Description = request.Description;
        module.ParentId = request.ParentId;
        module.SortOrder = request.SortOrder;
        module.UpdatedAt = DateTime.UtcNow;

        //var audit = new RbacAuditLog
        //{
        //    TenantId = tenantId,
        //    PerformedBy = _currentUserService.GetUserId() ?? 0,
        //    Resource = "Module",
        //    Action = "update",
        //    OldValues = oldValues,
        //    NewValues = System.Text.Json.JsonSerializer.Serialize(new { module.Name, module.Key, module.SortOrder, module.ParentId }),
        //    CreatedAt = DateTime.UtcNow
        //};
        //_dbContext.Set<RbacAuditLog>().Add(audit);

        return Result.Success(module.Adapt<ModuleResponse>());
    }
}