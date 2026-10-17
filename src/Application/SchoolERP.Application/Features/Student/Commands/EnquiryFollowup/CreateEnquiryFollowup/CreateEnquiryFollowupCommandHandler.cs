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

namespace SchoolERP.Application.Features.Student.Commands.EnquiryFollowup.CreateEnquiryFollowup;

public class CreateEnquiryFollowupCommandHandler : IRequestHandler<CreateEnquiryFollowupCommand, Result<EnquiryFollowupResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public CreateEnquiryFollowupCommandHandler(
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

    public async Task<Result<EnquiryFollowupResponse>> Handle(CreateEnquiryFollowupCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var branchId = _branchService.GetBranchId();
        var tenantId = _tenantService.GetTenantId();

        if (!branchId.HasValue || branchId.Value == Guid.Empty)
            return Error.Validation("Please select a branch to continue.");

        var enquiryErr = await _dbContext.EnsureUniqueAsync<EnquiryEntity>(
            e => e.Id == request.EnquiryId && e.BranchId == branchId.Value && !e.IsDeleted,
            "Enquiry not found.", cancellationToken);
        if (enquiryErr != null)
            return Error.NotFound("Enquiry", request.EnquiryId.ToString());

        var userId = _currentUserService.GetUserId() ?? 0;

        var followup = request.Adapt<EnquiryFollowupEntity>();
        followup.CreatedBy = userId;

        _dbContext.Set<EnquiryFollowupEntity>().Add(followup);

        //_dbContext.Set<RbacAuditLog>().Add(new RbacAuditLog
        //{
        //    TenantId = tenantId,
        //    PerformedBy = userId,
        //    Resource = "EnquiryFollowup",
        //    Action = "create",
        //    NewValues = System.Text.Json.JsonSerializer.Serialize(new { request.EnquiryId, request.ActivityType, request.NextFollowupDate }),
        //    CreatedAt = DateTime.UtcNow
        //});

        return Result.Success(followup.Adapt<EnquiryFollowupResponse>());
    }
}
