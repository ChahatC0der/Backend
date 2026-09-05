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

namespace SchoolERP.Application.Features.Academic.Commands.Section.CreateSection;

public class CreateSectionCommandHandler : IRequestHandler<CreateSectionCommand, Result<SectionResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public CreateSectionCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentTenantService tenantService,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<SectionResponse>> Handle(CreateSectionCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var branchId = _tenantService.GetBranchId();
        var tenantId = _tenantService.GetTenantId();

        // Check class exists
        var classExists = await _dbContext.EnsureEntityExistsAsync<ClassEntity>(request.ClassId, cancellationToken);
        if (classExists != null) return classExists;

        // Uniqueness: name unique within class
        var conflict = await _dbContext.EnsureUniqueAsync<SectionEntity>(
            s => s.ClassId == request.ClassId && s.Name == request.Name && !s.IsDeleted,
            $"Section '{request.Name}' already exists in this class.",
            cancellationToken);
        if (conflict != null) return conflict;

        var section = request.Adapt<SectionEntity>();
        _dbContext.Set<SectionEntity>().Add(section);

        // Audit
        var audit = new RbacAuditLog
        {
            TenantId = tenantId,
            PerformedBy = _currentUserService.GetUserId() ?? 0,
            Resource = "Section",
            Action = "create",
            NewValues = System.Text.Json.JsonSerializer.Serialize(new { request.ClassId, request.Name, request.Capacity }),
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Set<RbacAuditLog>().Add(audit);

        return Result.Success(section.Adapt<SectionResponse>());
    }
}