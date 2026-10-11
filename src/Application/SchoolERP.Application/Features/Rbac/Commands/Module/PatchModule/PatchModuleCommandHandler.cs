using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Rbac.DTOs;
using SchoolERP.Domain.Rbac.Entities;
using SchoolERP.Domain.Shared.Results;
using ModuleEntity = SchoolERP.Domain.Rbac.Entities.Module;

namespace SchoolERP.Application.Features.Rbac.Commands.Module.PatchModule;

public class PatchModuleCommandHandler : IRequestHandler<PatchModuleCommand, Result<ModuleResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public PatchModuleCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentTenantService tenantService,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<ModuleResponse>> Handle(PatchModuleCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var tenantId = _tenantService.GetTenantId();

        var module = await _dbContext.Set<ModuleEntity>()
            .FirstOrDefaultAsync(m => m.Id == request.Id && !m.IsDeleted && m.TenantId == tenantId, cancellationToken);
        if (module == null)
            return Error.NotFound("Module", request.Id.ToString());

        if (module.IsSystem)
            return Error.Conflict("System modules cannot be modified.");

        var oldValues = System.Text.Json.JsonSerializer.Serialize(new { module.Name, module.Key, module.SortOrder, module.ParentId, module.Description });

        if (request.ParentId.HasValue)
        {
            var parentExists = await _dbContext.EnsureEntityExistsAsync<ModuleEntity>(request.ParentId.Value, cancellationToken);
            if (parentExists != null) return Error.NotFound("Module", request.ParentId.Value.ToString());
            module.ParentId = request.ParentId;
        }

        request.Name.PatchIfProvided(v => module.Name = v);
        request.Description.PatchIfProvided(v => module.Description = v);

        if (!string.IsNullOrWhiteSpace(request.Key))
        {
            module.Key = request.Key.Trim().ToLowerInvariant();
            var conflict = await _dbContext.EnsureUniqueAsync<ModuleEntity>(
                m => m.TenantId == tenantId && m.Key == module.Key && m.Id != module.Id && !m.IsDeleted,
                $"Module with key '{module.Key}' already exists.",
                cancellationToken);
            if (conflict != null) return conflict;
        }

        if (request.SortOrder.HasValue) module.SortOrder = request.SortOrder.Value;
        module.UpdatedAt = DateTime.UtcNow;

        //var audit = new RbacAuditLog
        //{
        //    TenantId = tenantId,
        //    PerformedBy = _currentUserService.GetUserId() ?? 0,
        //    Resource = "Module",
        //    Action = "patch",
        //    OldValues = oldValues,
        //    NewValues = System.Text.Json.JsonSerializer.Serialize(new { module.Name, module.Key, module.SortOrder, module.ParentId, module.Description }),
        //    CreatedAt = DateTime.UtcNow
        //};
        //_dbContext.Set<RbacAuditLog>().Add(audit);

        return Result.Success(module.Adapt<ModuleResponse>());
    }
}