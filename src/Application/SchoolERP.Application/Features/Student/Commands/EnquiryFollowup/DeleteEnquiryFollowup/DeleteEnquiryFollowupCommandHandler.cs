using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Rbac.Entities;
using SchoolERP.Domain.Shared.Results;
using EnquiryEntity = SchoolERP.Domain.Student.Enquiry.Entities.Enquiry;
using EnquiryFollowupEntity = SchoolERP.Domain.Student.Enquiry.Entities.EnquiryFollowup;

namespace SchoolERP.Application.Features.Student.Commands.EnquiryFollowup.DeleteEnquiryFollowup;

public class DeleteEnquiryFollowupCommandHandler : IRequestHandler<DeleteEnquiryFollowupCommand, Result<bool>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public DeleteEnquiryFollowupCommandHandler(
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

    public async Task<Result<bool>> Handle(DeleteEnquiryFollowupCommand command, CancellationToken cancellationToken)
    {
        var branchId = _branchService.GetBranchId();
        var tenantId = _tenantService.GetTenantId();

        if (!branchId.HasValue || branchId.Value == Guid.Empty)
            return Error.Validation("Please select a branch to continue.");

        var followup = await (
            from f in _dbContext.Set<EnquiryFollowupEntity>()
            join e in _dbContext.Set<EnquiryEntity>() on f.EnquiryId equals e.Id
            where f.Id == command.Id && !f.IsDeleted && !e.IsDeleted && e.BranchId == branchId.Value
            select f
        ).FirstOrDefaultAsync(cancellationToken);

        if (followup == null)
            return Error.NotFound("EnquiryFollowup", command.Id.ToString());

        var userId = _currentUserService.GetUserId() ?? 0;

        followup.IsDeleted = true;
        followup.DeletedAt = DateTime.UtcNow;
        followup.UpdatedAt = DateTime.UtcNow;

        //_dbContext.Set<RbacAuditLog>().Add(new RbacAuditLog
        //{
        //    TenantId = tenantId,
        //    PerformedBy = userId,
        //    Resource = "EnquiryFollowup",
        //    Action = "delete",
        //    OldValues = System.Text.Json.JsonSerializer.Serialize(new { followup.EnquiryId, followup.ActivityType, followup.Notes }),
        //    CreatedAt = DateTime.UtcNow
        //});

        return Result.Success(true);
    }
}
