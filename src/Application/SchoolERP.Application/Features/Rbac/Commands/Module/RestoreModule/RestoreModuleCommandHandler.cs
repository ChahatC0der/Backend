using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Rbac.Entities;
using SchoolERP.Domain.Shared.Results;
using ModuleEntity = SchoolERP.Domain.Rbac.Entities.Module;

namespace SchoolERP.Application.Features.Rbac.Commands.Module.RestoreModule;

public class RestoreModuleCommandHandler : IRequestHandler<RestoreModuleCommand, Result<bool>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public RestoreModuleCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentTenantService tenantService,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<bool>> Handle(RestoreModuleCommand command, CancellationToken cancellationToken)
    {
        var tenantId = _tenantService.GetTenantId();

        var module = await _dbContext.Set<ModuleEntity>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(m => m.Id == command.Id && m.IsDeleted && m.TenantId == tenantId, cancellationToken);
        if (module == null)
            return Error.NotFound("Module", command.Id.ToString());

        module.IsDeleted = false;
        module.DeletedAt = null;
        module.UpdatedAt = DateTime.UtcNow;

        //var audit = new RbacAuditLog
        //{
        //    TenantId = tenantId,
        //    PerformedBy = _currentUserService.GetUserId() ?? 0,
        //    Resource = "Module",
        //    Action = "restore",
        //    NewValues = System.Text.Json.JsonSerializer.Serialize(new { module.Id, module.Name, module.Key }),
        //    CreatedAt = DateTime.UtcNow
        //};
        //_dbContext.Set<RbacAuditLog>().Add(audit);

        return Result.Success(true);
    }
}