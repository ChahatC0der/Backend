using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Student.DTOs;
using SchoolERP.Domain.Rbac.Entities;
using SchoolERP.Domain.Shared.Results;
using RegistrationEntity = SchoolERP.Domain.Student.Registration.Entities.Registration;
using RegistrationDocumentEntity = SchoolERP.Domain.Student.Registration.Entities.RegistrationDocument;

namespace SchoolERP.Application.Features.Student.Commands.RegistrationDocument.CreateRegistrationDocument;

public class CreateRegistrationDocumentCommandHandler : IRequestHandler<CreateRegistrationDocumentCommand, Result<RegistrationDocumentResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public CreateRegistrationDocumentCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentBranchService branchService,
        ICurrentTenantService tenantService,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _branchService = branchService;
        _tenantService = tenantService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<RegistrationDocumentResponse>> Handle(CreateRegistrationDocumentCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var branchId = _branchService.GetBranchId();
        var tenantId = _tenantService.GetTenantId();

        if (!branchId.HasValue || branchId.Value == Guid.Empty)
            return Error.Validation("Please select a branch to continue.");

        // Validate registration belongs to branch
        var regErr = await _dbContext.EnsureUniqueAsync<RegistrationEntity>(
            r => r.Id == request.RegistrationId && r.BranchId == branchId.Value && !r.IsDeleted,
            "Registration not found.", cancellationToken);
        if (regErr != null)
            return Error.NotFound("Registration", request.RegistrationId.ToString());

        var doc = request.Adapt<RegistrationDocumentEntity>();
        doc.Status = "uploaded";

        _dbContext.Set<RegistrationDocumentEntity>().Add(doc);

        var userId = _currentUserService.GetUserId() ?? 0;
        _dbContext.Set<RbacAuditLog>().Add(new RbacAuditLog
        {
            TenantId = tenantId,
            PerformedBy = userId,
            Resource = "RegistrationDocument",
            Action = "create",
            NewValues = System.Text.Json.JsonSerializer.Serialize(new { request.RegistrationId, request.DocumentType, request.FileUrl }),
            CreatedAt = DateTime.UtcNow
        });

        return Result.Success(doc.Adapt<RegistrationDocumentResponse>());
    }
}
