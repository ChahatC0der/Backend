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

namespace SchoolERP.Application.Features.Student.Commands.RegistrationDocument.UpdateRegistrationDocument;

public class UpdateRegistrationDocumentCommandHandler : IRequestHandler<UpdateRegistrationDocumentCommand, Result<RegistrationDocumentResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public UpdateRegistrationDocumentCommandHandler(
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

    public async Task<Result<RegistrationDocumentResponse>> Handle(UpdateRegistrationDocumentCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var branchId = _branchService.GetBranchId();
        var tenantId = _tenantService.GetTenantId();

        if (!branchId.HasValue || branchId.Value == Guid.Empty)
            return Error.Validation("Please select a branch to continue.");

        var doc = await (
            from d in _dbContext.Set<RegistrationDocumentEntity>()
            join r in _dbContext.Set<RegistrationEntity>() on d.RegistrationId equals r.Id
            where d.Id == request.Id && !d.IsDeleted && !r.IsDeleted && r.BranchId == branchId.Value
            select d
        ).FirstOrDefaultAsync(cancellationToken);

        if (doc == null)
            return Error.NotFound("RegistrationDocument", request.Id.ToString());

        var oldValues = System.Text.Json.JsonSerializer.Serialize(new { doc.DocumentType, doc.FileUrl, doc.Status });

        doc.DocumentType = request.DocumentType;
        doc.FileUrl = request.FileUrl;
        doc.Description = request.Description;
        doc.Status = request.Status;
        doc.RejectionReason = request.RejectionReason;
        doc.UpdatedAt = DateTime.UtcNow;

        var userId = _currentUserService.GetUserId() ?? 0;
        _dbContext.Set<RbacAuditLog>().Add(new RbacAuditLog
        {
            TenantId = tenantId,
            PerformedBy = userId,
            Resource = "RegistrationDocument",
            Action = "update",
            OldValues = oldValues,
            NewValues = System.Text.Json.JsonSerializer.Serialize(new { doc.DocumentType, doc.FileUrl, doc.Status }),
            CreatedAt = DateTime.UtcNow
        });

        return Result.Success(doc.Adapt<RegistrationDocumentResponse>());
    }
}
