using MediatR;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Rbac.Entities;
using SchoolERP.Domain.Shared.Results;
using AdmissionEntity = SchoolERP.Domain.Student.Admission.Entities.Admission;

namespace SchoolERP.Application.Features.Student.Commands.Admission.DeleteAdmission;

public class DeleteAdmissionCommandHandler : IRequestHandler<DeleteAdmissionCommand, Result<bool>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public DeleteAdmissionCommandHandler(
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

    public async Task<Result<bool>> Handle(DeleteAdmissionCommand command, CancellationToken cancellationToken)
    {
        var branchId = _branchService.GetBranchId();
        var tenantId = _tenantService.GetTenantId();

        if (branchId == null) return Error.Conflict("Please Choose a Branch");

        var entityResult = await _dbContext.GetEntityAsync<AdmissionEntity>(
            a => a.Id == command.Id && a.BranchId == branchId && !a.IsDeleted,
            "Admission", command.Id.ToString(), cancellationToken);
        if (entityResult.IsFailure) return entityResult.Error;

        var admission = entityResult.Value;

        if (admission.Status == "enrolled")
            return Error.Conflict("Cannot delete an enrolled admission.");

        admission.IsDeleted = true;
        admission.DeletedAt = DateTime.UtcNow;
        admission.UpdatedAt = DateTime.UtcNow;

        var userId = _currentUserService.GetUserId() ?? 0;
        _dbContext.Set<RbacAuditLog>().Add(new RbacAuditLog
        {
            TenantId = tenantId,
            PerformedBy = userId,
            Resource = "Admission",
            Action = "delete",
            OldValues = System.Text.Json.JsonSerializer.Serialize(new { admission.RegistrationId, admission.EnquiryId, admission.Status }),
            CreatedAt = DateTime.UtcNow
        });

        return Result.Success(true);
    }
}
