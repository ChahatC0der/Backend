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

namespace SchoolERP.Application.Features.Academic.Commands.Section.PatchSection;

public class PatchSectionCommandHandler : IRequestHandler<PatchSectionCommand, Result<SectionResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public PatchSectionCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentTenantService tenantService,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<SectionResponse>> Handle(PatchSectionCommand command, CancellationToken cancellationToken)
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
        var oldValues = System.Text.Json.JsonSerializer.Serialize(new { section.ClassId, section.Name, section.Capacity });

        if (request.ClassId.HasValue)
        {
            var classExists = await _dbContext.EnsureEntityExistsAsync<ClassEntity>(request.ClassId.Value, cancellationToken);
            if (classExists != null) return classExists;
            section.ClassId = request.ClassId.Value;
        }

        request.Name.PatchIfProvided(value => section.Name = value);

        if (request.Capacity.HasValue) section.Capacity = request.Capacity.Value;

        // If name or class changed, check uniqueness
        if (!string.IsNullOrWhiteSpace(request.Name) || request.ClassId.HasValue)
        {
            var conflict = await _dbContext.EnsureUniqueAsync<SectionEntity>(
                s => s.ClassId == section.ClassId && s.Name == section.Name && s.Id != section.Id && !s.IsDeleted,
                $"Section '{section.Name}' already exists in this class.",
                cancellationToken);
            if (conflict != null) return conflict;
        }

        section.UpdatedAt = DateTime.UtcNow;

        var audit = new RbacAuditLog
        {
            TenantId = tenantId,
            PerformedBy = _currentUserService.GetUserId() ?? 0,
            Resource = "Section",
            Action = "patch",
            OldValues = oldValues,
            NewValues = System.Text.Json.JsonSerializer.Serialize(new { section.ClassId, section.Name, section.Capacity }),
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Set<RbacAuditLog>().Add(audit);

        return Result.Success(section.Adapt<SectionResponse>());
    }
}