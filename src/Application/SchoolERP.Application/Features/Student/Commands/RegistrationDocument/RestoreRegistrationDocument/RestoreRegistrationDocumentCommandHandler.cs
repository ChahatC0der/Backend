using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Rbac.Entities;
using SchoolERP.Domain.Shared.Results;
using RegistrationDocumentEntity = SchoolERP.Domain.Student.Registration.Entities.RegistrationDocument;

namespace SchoolERP.Application.Features.Student.Commands.RegistrationDocument.RestoreRegistrationDocument;

public class RestoreRegistrationDocumentCommandHandler : IRequestHandler<RestoreRegistrationDocumentCommand, Result<bool>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public RestoreRegistrationDocumentCommandHandler(
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

    public async Task<Result<bool>> Handle(RestoreRegistrationDocumentCommand command, CancellationToken cancellationToken)
    {
        var branchId = _branchService.GetBranchId();
        var tenantId = _tenantService.GetTenantId();

        if (!branchId.HasValue || branchId.Value == Guid.Empty)
            return Error.Validation("Please select a branch to continue.");

        var doc = await _dbContext.Set<RegistrationDocumentEntity>()
            .IgnoreQueryFilters()
            .Include(d => d.Registration)
            .FirstOrDefaultAsync(d => d.Id == command.Id && d.IsDeleted, cancellationToken);

        if (doc == null)
            return Error.NotFound("RegistrationDocument", command.Id.ToString());

        if (doc.Registration == null || doc.Registration.BranchId != branchId.Value || doc.Registration.IsDeleted)
            return Error.NotFound("RegistrationDocument", command.Id.ToString());

        doc.IsDeleted = false;
        doc.DeletedAt = null;
        doc.UpdatedAt = DateTime.UtcNow;

        var userId = _currentUserService.GetUserId() ?? 0;
        _dbContext.Set<RbacAuditLog>().Add(new RbacAuditLog
        {
            TenantId = tenantId,
            PerformedBy = userId,
            Resource = "RegistrationDocument",
            Action = "restore",
            NewValues = System.Text.Json.JsonSerializer.Serialize(new { doc.Id, doc.RegistrationId, doc.DocumentType }),
            CreatedAt = DateTime.UtcNow
        });

        return Result.Success(true);
    }
}
