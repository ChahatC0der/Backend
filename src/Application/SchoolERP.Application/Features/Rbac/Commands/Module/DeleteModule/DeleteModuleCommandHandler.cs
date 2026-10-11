using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using PermissionEntity = SchoolERP.Domain.Rbac.Entities.Permission;
using SchoolERP.Domain.Shared.Results;
using ModuleEntity = SchoolERP.Domain.Rbac.Entities.Module;

namespace SchoolERP.Application.Features.Rbac.Commands.Module.DeleteModule;

public class DeleteModuleCommandHandler : IRequestHandler<DeleteModuleCommand, Result<bool>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public DeleteModuleCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentTenantService tenantService,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<bool>> Handle(DeleteModuleCommand command, CancellationToken cancellationToken)
    {
        var tenantId = _tenantService.GetTenantId();

        var module = await _dbContext.Set<ModuleEntity>()
            .FirstOrDefaultAsync(m => m.Id == command.Id && !m.IsDeleted && m.TenantId == tenantId, cancellationToken);
        if (module == null)
            return Error.NotFound("Module", command.Id.ToString());

        if (module.IsSystem)
            return Error.Conflict("System modules cannot be deleted.");

        // Check for dependent permissions
        var hasPermissions = await _dbContext.Set<PermissionEntity>()
            .AnyAsync(p => p.ModuleId == module.Id , cancellationToken);
        if (hasPermissions)
            return Error.Conflict("Cannot delete module with existing permissions. Delete permissions first.");

        module.IsDeleted = true;
        module.DeletedAt = DateTime.UtcNow;
        module.UpdatedAt = DateTime.UtcNow;

        //var audit = new RbacAuditLog
        //{
        //    TenantId = tenantId,
        //    PerformedBy = _currentUserService.GetUserId() ?? 0,
        //    Resource = "Module",
        //    Action = "delete",
        //    OldValues = System.Text.Json.JsonSerializer.Serialize(new { module.Name, module.Key }),
        //    CreatedAt = DateTime.UtcNow
        //};
        //_dbContext.Set<RbacAuditLog>().Add(audit);

        return Result.Success(true);
    }
}