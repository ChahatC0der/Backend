using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Rbac.Entities;
using SchoolERP.Domain.Shared.Results;
using AdmissionEntity = SchoolERP.Domain.Student.Admission.Entities.Admission;

namespace SchoolERP.Application.Features.Student.Commands.Admission.RestoreAdmission;

public class RestoreAdmissionCommandHandler : IRequestHandler<RestoreAdmissionCommand, Result<bool>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public RestoreAdmissionCommandHandler(
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

    public async Task<Result<bool>> Handle(RestoreAdmissionCommand command, CancellationToken cancellationToken)
    {
        var branchId = _branchService.GetBranchId();
        var tenantId = _tenantService.GetTenantId();

        if (branchId == null) return Error.Conflict("Please Choose a Branch");

        var admission = await _dbContext.Set<AdmissionEntity>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(a => a.Id == command.Id && a.BranchId == branchId && a.IsDeleted, cancellationToken);

        if (admission == null)
            return Error.NotFound("Admission", command.Id.ToString());

        admission.IsDeleted = false;
        admission.DeletedAt = null;
        admission.UpdatedAt = DateTime.UtcNow;

        var userId = _currentUserService.GetUserId() ?? 0;
        _dbContext.Set<RbacAuditLog>().Add(new RbacAuditLog
        {
            TenantId = tenantId,
            PerformedBy = userId,
            Resource = "Admission",
            Action = "restore",
            NewValues = System.Text.Json.JsonSerializer.Serialize(new { admission.Id, admission.RegistrationId, admission.EnquiryId }),
            CreatedAt = DateTime.UtcNow
        });

        return Result.Success(true);
    }
}
