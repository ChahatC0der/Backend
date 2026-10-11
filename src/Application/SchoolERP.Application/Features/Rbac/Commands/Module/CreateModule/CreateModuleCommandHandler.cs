using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Rbac.DTOs;
using SchoolERP.Domain.Rbac.Entities;
using SchoolERP.Domain.Shared.Results;
using ModuleEntity = SchoolERP.Domain.Rbac.Entities.Module;

namespace SchoolERP.Application.Features.Rbac.Commands.Module.CreateModule;

public class CreateModuleCommandHandler : IRequestHandler<CreateModuleCommand, Result<ModuleResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public CreateModuleCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentTenantService tenantService,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<ModuleResponse>> Handle(CreateModuleCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var tenantId = _tenantService.GetTenantId();

        // Parent existence check
        if (request.ParentId.HasValue)
        {
            var parentExists = await _dbContext.EnsureEntityExistsAsync<ModuleEntity>(request.ParentId.Value, cancellationToken);
            if (parentExists != null) return Error.NotFound("Module", request.ParentId.Value.ToString());
        }

        // Uniqueness: Key unique per tenant
        var conflict = await _dbContext.EnsureUniqueAsync<ModuleEntity>(
            m => m.TenantId == tenantId && m.Key == request.Key && !m.IsDeleted,
            $"Module with key '{request.Key}' already exists.",
            cancellationToken);
        if (conflict != null) return conflict;

        var module = request.Adapt<ModuleEntity>();
        module.TenantId = tenantId;    // tenant-specific (system modules seeded separately)
        module.IsSystem = false;

        _dbContext.Set<ModuleEntity>().Add(module);

        // Audit log
        //var audit = new RbacAuditLog
        //{
        //    TenantId = tenantId,
        //    PerformedBy = _currentUserService.GetUserId() ?? 0,
        //    Resource = "Module",
        //    Action = "create",
        //    NewValues = System.Text.Json.JsonSerializer.Serialize(new { request.Name, request.Key, request.SortOrder }),
        //    CreatedAt = DateTime.UtcNow
        //};
        //_dbContext.Set<RbacAuditLog>().Add(audit);

        return Result.Success(module.Adapt<ModuleResponse>());
    }
}