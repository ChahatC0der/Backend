using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Student.DTOs;
using SchoolERP.Domain.Rbac.Entities;
using SchoolERP.Domain.Shared.Results;
using ClassEntity = SchoolERP.Domain.Academic.Entities.Class;
using EnquiryEntity = SchoolERP.Domain.Student.Enquiry.Entities.Enquiry;
using EnquiryStatusLogEntity = SchoolERP.Domain.Student.Enquiry.Entities.EnquiryStatusLog;
using UserEntity = SchoolERP.Domain.Rbac.Entities.User;

namespace SchoolERP.Application.Features.Student.Commands.Enquiry.ChangeEnquiryStatus;

public class ChangeEnquiryStatusCommandHandler : IRequestHandler<ChangeEnquiryStatusCommand, Result<EnquiryResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public ChangeEnquiryStatusCommandHandler(
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

    public async Task<Result<EnquiryResponse>> Handle(ChangeEnquiryStatusCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var branchId = _branchService.GetBranchId();
        var tenantId = _tenantService.GetTenantId();

        if (!branchId.HasValue || branchId.Value == Guid.Empty)
            return Error.Validation("Please select a branch to continue.");

        var entityResult = await _dbContext.GetEntityAsync<EnquiryEntity>(
            e => e.Id == request.Id && e.BranchId == branchId.Value && !e.IsDeleted,
            "Enquiry", request.Id.ToString(), cancellationToken);
        if (entityResult.IsFailure) return entityResult.Error;

        var enquiry = entityResult.Value;
        var oldStatus = enquiry.Status;

        if (oldStatus == request.NewStatus)
            return Error.Validation("Status is already set to this value.");

        enquiry.Status = request.NewStatus;
        enquiry.UpdatedAt = DateTime.UtcNow;

        var userId = _currentUserService.GetUserId() ?? 0;

        //_dbContext.Set<EnquiryStatusLogEntity>().Add(new EnquiryStatusLogEntity
        //{
        //    EnquiryId = enquiry.Id,
        //    OldStatus = oldStatus,
        //    NewStatus = request.NewStatus,
        //    Reason = request.Reason,
        //    PerformedBy = userId
        //});

        //_dbContext.Set<RbacAuditLog>().Add(new RbacAuditLog
        //{
        //    TenantId = tenantId,
        //    PerformedBy = userId,
        //    Resource = "Enquiry",
        //    Action = "status_change",
        //    OldValues = System.Text.Json.JsonSerializer.Serialize(new { Status = oldStatus }),
        //    NewValues = System.Text.Json.JsonSerializer.Serialize(new { Status = request.NewStatus, Reason = request.Reason }),
        //    CreatedAt = DateTime.UtcNow
        //});

        var response = enquiry.Adapt<EnquiryResponse>();

        string? className = null, assignedToName = null;
        if (response.ClassId.HasValue)
            className = await _dbContext.Set<ClassEntity>().AsNoTracking().Where(c => c.Id == response.ClassId.Value).Select(c => c.Name).FirstOrDefaultAsync(cancellationToken);
        if (response.AssignedTo.HasValue)
            assignedToName = await _dbContext.Set<UserEntity>().AsNoTracking().Where(u => u.Id == response.AssignedTo.Value).Select(u => u.Name).FirstOrDefaultAsync(cancellationToken);

        return Result.Success(response with { ClassName = className, AssignedToName = assignedToName });
    }
}
