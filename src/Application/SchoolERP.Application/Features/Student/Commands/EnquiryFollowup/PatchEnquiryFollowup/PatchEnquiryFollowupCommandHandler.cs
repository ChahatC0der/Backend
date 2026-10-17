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

namespace SchoolERP.Application.Features.Student.Commands.EnquiryFollowup.PatchEnquiryFollowup;

public class PatchEnquiryFollowupCommandHandler : IRequestHandler<PatchEnquiryFollowupCommand, Result<EnquiryFollowupResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public PatchEnquiryFollowupCommandHandler(
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

    public async Task<Result<EnquiryFollowupResponse>> Handle(PatchEnquiryFollowupCommand command, CancellationToken cancellationToken)
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

        var oldValues = System.Text.Json.JsonSerializer.Serialize(new { followup.EnquiryId, followup.ActivityType, followup.Notes, followup.NextFollowupDate });

        if (request.EnquiryId.HasValue && request.EnquiryId.Value != followup.EnquiryId)
        {
            var newEnqErr = await _dbContext.EnsureUniqueAsync<EnquiryEntity>(
                e => e.Id == request.EnquiryId.Value && e.BranchId == branchId.Value && !e.IsDeleted,
                "Enquiry not found.", cancellationToken);
            if (newEnqErr != null)
                return Error.NotFound("Enquiry", request.EnquiryId.Value.ToString());
            followup.EnquiryId = request.EnquiryId.Value;
        }

        request.ActivityType.PatchIfProvided(v => followup.ActivityType = v);
        request.Notes.PatchIfProvided(v => followup.Notes = v);
        if (request.NextFollowupDate.HasValue) followup.NextFollowupDate = request.NextFollowupDate.Value;

        followup.UpdatedAt = DateTime.UtcNow;

        var userId = _currentUserService.GetUserId() ?? 0;
        //_dbContext.Set<RbacAuditLog>().Add(new RbacAuditLog
        //{
        //    TenantId = tenantId,
        //    PerformedBy = userId,
        //    Resource = "EnquiryFollowup",
        //    Action = "patch",
        //    OldValues = oldValues,
        //    NewValues = System.Text.Json.JsonSerializer.Serialize(new { followup.EnquiryId, followup.ActivityType, followup.Notes, followup.NextFollowupDate }),
        //    CreatedAt = DateTime.UtcNow
        //});

        return Result.Success(followup.Adapt<EnquiryFollowupResponse>());
    }
}
