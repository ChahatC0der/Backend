using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Academic.DTOs;
using ClassEntity = SchoolERP.Domain.Academic.Entities.Class;
using SectionEntity = SchoolERP.Domain.Academic.Entities.Section;
using SchoolERP.Domain.Rbac.Entities;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Academic.Commands.Section.UpdateSection;

public class UpdateSectionCommandHandler : IRequestHandler<UpdateSectionCommand, Result<SectionResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public UpdateSectionCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentTenantService tenantService,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<SectionResponse>> Handle(UpdateSectionCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var branchId = _tenantService.GetBranchId();
        var tenantId = _tenantService.GetTenantId();

        var entityResult = await _dbContext.GetEntityAsync<SectionEntity>(
            s => s.Id == request.Id && s.BranchId == branchId && !s.IsDeleted,
            "Section",
            request.Id.ToString(),
            cancellationToken);
        if (entityResult.IsFailure) return entityResult.Error;

        var section = entityResult.Value;

        // Check class exists
        var classExists = await _dbContext.EnsureEntityExistsAsync<ClassEntity>(request.ClassId, cancellationToken);
        if (classExists != null) return classExists;

        // Uniqueness: name unique within class (excluding current)
        var conflict = await _dbContext.EnsureUniqueAsync<SectionEntity>(
            s => s.ClassId == request.ClassId && s.Name == request.Name && s.Id != request.Id && !s.IsDeleted,
            $"Section '{request.Name}' already exists in this class.",
            cancellationToken);
        if (conflict != null) return conflict;

        var oldValues = System.Text.Json.JsonSerializer.Serialize(new { section.ClassId, section.Name, section.Capacity });

        section.ClassId = request.ClassId;
        section.Name = request.Name;
        section.Capacity = request.Capacity;
        section.UpdatedAt = DateTime.UtcNow;

        var audit = new RbacAuditLog
        {
            TenantId = tenantId,
            PerformedBy = _currentUserService.GetUserId() ?? 0,
            Resource = "Section",
            Action = "update",
            OldValues = oldValues,
            NewValues = System.Text.Json.JsonSerializer.Serialize(new { section.ClassId, section.Name, section.Capacity }),
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Set<RbacAuditLog>().Add(audit);

        return Result.Success(section.Adapt<SectionResponse>());
    }
}