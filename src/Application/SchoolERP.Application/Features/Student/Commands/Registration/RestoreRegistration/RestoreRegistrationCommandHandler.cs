using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Rbac.Entities;
using SchoolERP.Domain.Shared.Results;
using RegistrationEntity = SchoolERP.Domain.Student.Registration.Entities.Registration;

namespace SchoolERP.Application.Features.Student.Commands.Registration.RestoreRegistration;

public class RestoreRegistrationCommandHandler : IRequestHandler<RestoreRegistrationCommand, Result<bool>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public RestoreRegistrationCommandHandler(
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

    public async Task<Result<bool>> Handle(RestoreRegistrationCommand command, CancellationToken cancellationToken)
    {
        var branchId = _branchService.GetBranchId();
        var tenantId = _tenantService.GetTenantId();

        if (!branchId.HasValue || branchId.Value == Guid.Empty)
            return Error.Validation("Please select a branch to continue.");

        var registration = await _dbContext.Set<RegistrationEntity>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(r => r.Id == command.Id && r.BranchId == branchId.Value && r.IsDeleted, cancellationToken);

        if (registration == null)
            return Error.NotFound("Registration", command.Id.ToString());

        registration.IsDeleted = false;
        registration.DeletedAt = null;
        registration.UpdatedAt = DateTime.UtcNow;

        var userId = _currentUserService.GetUserId() ?? 0;
        _dbContext.Set<RbacAuditLog>().Add(new RbacAuditLog
        {
            TenantId = tenantId,
            PerformedBy = userId,
            Resource = "Registration",
            Action = "restore",
            NewValues = System.Text.Json.JsonSerializer.Serialize(new
            {
                registration.Id,
                registration.RegistrationNumber
            }),
            CreatedAt = DateTime.UtcNow
        });

        return Result.Success(true);
    }
}
