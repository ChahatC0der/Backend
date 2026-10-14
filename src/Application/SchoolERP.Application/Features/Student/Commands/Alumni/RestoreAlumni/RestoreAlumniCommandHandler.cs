using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Rbac.Entities;
using SchoolERP.Domain.Shared.Results;
using AlumniEntity = SchoolERP.Domain.Student.Alumni.Entities.Alumni;

namespace SchoolERP.Application.Features.Student.Commands.Alumni.RestoreAlumni;

public class RestoreAlumniCommandHandler : IRequestHandler<RestoreAlumniCommand, Result<bool>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public RestoreAlumniCommandHandler(
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

    public async Task<Result<bool>> Handle(RestoreAlumniCommand command, CancellationToken cancellationToken)
    {
        var branchId = _branchService.GetBranchId();
        var tenantId = _tenantService.GetTenantId();

        if (branchId == null) return Error.Conflict("Please Choose a Branch");

        var alumni = await _dbContext.Set<AlumniEntity>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(a => a.Id == command.Id && a.BranchId == branchId && a.IsDeleted, cancellationToken);

        if (alumni == null)
            return Error.NotFound("Alumni", command.Id.ToString());

        alumni.IsDeleted = false;
        alumni.DeletedAt = null;
        alumni.UpdatedAt = DateTime.UtcNow;

        var userId = _currentUserService.GetUserId() ?? 0;
        _dbContext.Set<RbacAuditLog>().Add(new RbacAuditLog
        {
            TenantId = tenantId,
            PerformedBy = userId,
            Resource = "Alumni",
            Action = "restore",
            NewValues = System.Text.Json.JsonSerializer.Serialize(new { alumni.Id, alumni.AlumniId }),
            CreatedAt = DateTime.UtcNow
        });

        return Result.Success(true);
    }
}
