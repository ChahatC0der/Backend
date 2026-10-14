using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Rbac.Entities;
using SchoolERP.Domain.Shared.Results;
using EnquiryFollowupEntity = SchoolERP.Domain.Student.Enquiry.Entities.EnquiryFollowup;

namespace SchoolERP.Application.Features.Student.Commands.EnquiryFollowup.RestoreEnquiryFollowup;

public class RestoreEnquiryFollowupCommandHandler : IRequestHandler<RestoreEnquiryFollowupCommand, Result<bool>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public RestoreEnquiryFollowupCommandHandler(
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

    public async Task<Result<bool>> Handle(RestoreEnquiryFollowupCommand command, CancellationToken cancellationToken)
    {
        var branchId = _branchService.GetBranchId();
        var tenantId = _tenantService.GetTenantId();

        if (!branchId.HasValue || branchId.Value == Guid.Empty)
            return Error.Validation("Please select a branch to continue.");

        var followup = await _dbContext.Set<EnquiryFollowupEntity>()
            .IgnoreQueryFilters()
            .Include(f => f.Enquiry)
            .FirstOrDefaultAsync(f => f.Id == command.Id && f.IsDeleted, cancellationToken);

        if (followup == null)
            return Error.NotFound("EnquiryFollowup", command.Id.ToString());

        if (followup.Enquiry == null || followup.Enquiry.BranchId != branchId.Value || followup.Enquiry.IsDeleted)
            return Error.NotFound("EnquiryFollowup", command.Id.ToString());

        followup.IsDeleted = false;
        followup.DeletedAt = null;
        followup.UpdatedAt = DateTime.UtcNow;

        var userId = _currentUserService.GetUserId() ?? 0;
        _dbContext.Set<RbacAuditLog>().Add(new RbacAuditLog
        {
            TenantId = tenantId,
            PerformedBy = userId,
            Resource = "EnquiryFollowup",
            Action = "restore",
            NewValues = System.Text.Json.JsonSerializer.Serialize(new { followup.Id, followup.EnquiryId, followup.ActivityType }),
            CreatedAt = DateTime.UtcNow
        });

        return Result.Success(true);
    }
}
