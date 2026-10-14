using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Student.DTOs;
using SchoolERP.Domain.Rbac.Entities;
using SchoolERP.Domain.Shared.Results;
using EnquiryEntity = SchoolERP.Domain.Student.Enquiry.Entities.Enquiry;
using EnquiryFollowupEntity = SchoolERP.Domain.Student.Enquiry.Entities.EnquiryFollowup;

namespace SchoolERP.Application.Features.Student.Commands.EnquiryFollowup.UpdateEnquiryFollowup;

public class UpdateEnquiryFollowupCommandHandler : IRequestHandler<UpdateEnquiryFollowupCommand, Result<EnquiryFollowupResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public UpdateEnquiryFollowupCommandHandler(
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

    public async Task<Result<EnquiryFollowupResponse>> Handle(UpdateEnquiryFollowupCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var branchId = _branchService.GetBranchId();
        var tenantId = _tenantService.GetTenantId();

        if (!branchId.HasValue || branchId.Value == Guid.Empty)
            return Error.Validation("Please select a branch to continue.");

        var followup = await (
            from f in _dbContext.Set<EnquiryFollowupEntity>()
            join e in _dbContext.Set<EnquiryEntity>() on f.EnquiryId equals e.Id
            where f.Id == request.Id && !f.IsDeleted && !e.IsDeleted && e.BranchId == branchId.Value
            select f
        ).FirstOrDefaultAsync(cancellationToken);

        if (followup == null)
            return Error.NotFound("EnquiryFollowup", request.Id.ToString());

        if (request.EnquiryId != followup.EnquiryId)
        {
            var newEnqErr = await _dbContext.EnsureUniqueAsync<EnquiryEntity>(
                e => e.Id == request.EnquiryId && e.BranchId == branchId.Value && !e.IsDeleted,
                "Enquiry not found.", cancellationToken);
            if (newEnqErr != null)
                return Error.NotFound("Enquiry", request.EnquiryId.ToString());
        }

        var userId = _currentUserService.GetUserId() ?? 0;

        var oldValues = System.Text.Json.JsonSerializer.Serialize(new { followup.ActivityType, followup.Notes, followup.NextFollowupDate });

        followup.EnquiryId = request.EnquiryId;
        followup.ActivityType = request.ActivityType;
        followup.Notes = request.Notes;
        followup.NextFollowupDate = request.NextFollowupDate;
        followup.UpdatedAt = DateTime.UtcNow;

        _dbContext.Set<RbacAuditLog>().Add(new RbacAuditLog
        {
            TenantId = tenantId,
            PerformedBy = userId,
            Resource = "EnquiryFollowup",
            Action = "update",
            OldValues = oldValues,
            NewValues = System.Text.Json.JsonSerializer.Serialize(new { followup.EnquiryId, followup.ActivityType, followup.Notes, followup.NextFollowupDate }),
            CreatedAt = DateTime.UtcNow
        });

        return Result.Success(followup.Adapt<EnquiryFollowupResponse>());
    }
}
