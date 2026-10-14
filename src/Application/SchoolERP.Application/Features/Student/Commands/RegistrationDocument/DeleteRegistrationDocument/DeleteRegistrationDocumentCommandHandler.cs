using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Rbac.Entities;
using SchoolERP.Domain.Shared.Results;
using RegistrationEntity = SchoolERP.Domain.Student.Registration.Entities.Registration;
using RegistrationDocumentEntity = SchoolERP.Domain.Student.Registration.Entities.RegistrationDocument;

namespace SchoolERP.Application.Features.Student.Commands.RegistrationDocument.DeleteRegistrationDocument;

public class DeleteRegistrationDocumentCommandHandler : IRequestHandler<DeleteRegistrationDocumentCommand, Result<bool>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public DeleteRegistrationDocumentCommandHandler(
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

    public async Task<Result<bool>> Handle(DeleteRegistrationDocumentCommand command, CancellationToken cancellationToken)
    {
        var branchId = _branchService.GetBranchId();
        var tenantId = _tenantService.GetTenantId();

        if (!branchId.HasValue || branchId.Value == Guid.Empty)
            return Error.Validation("Please select a branch to continue.");

        var doc = await (
            from d in _dbContext.Set<RegistrationDocumentEntity>()
            join r in _dbContext.Set<RegistrationEntity>() on d.RegistrationId equals r.Id
            where d.Id == command.Id && !d.IsDeleted && !r.IsDeleted && r.BranchId == branchId.Value
            select d
        ).FirstOrDefaultAsync(cancellationToken);

        if (doc == null)
            return Error.NotFound("RegistrationDocument", command.Id.ToString());

        doc.IsDeleted = true;
        doc.DeletedAt = DateTime.UtcNow;
        doc.UpdatedAt = DateTime.UtcNow;

        var userId = _currentUserService.GetUserId() ?? 0;
        _dbContext.Set<RbacAuditLog>().Add(new RbacAuditLog
        {
            TenantId = tenantId,
            PerformedBy = userId,
            Resource = "RegistrationDocument",
            Action = "delete",
            OldValues = System.Text.Json.JsonSerializer.Serialize(new { doc.RegistrationId, doc.DocumentType, doc.FileUrl }),
            CreatedAt = DateTime.UtcNow
        });

        return Result.Success(true);
    }
}
