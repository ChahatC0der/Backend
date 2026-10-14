using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Student.DTOs;
using SchoolERP.Domain.Rbac.Entities;
using SchoolERP.Domain.Shared.Results;
using ClassEntity = SchoolERP.Domain.Academic.Entities.Class;
using SectionEntity = SchoolERP.Domain.Academic.Entities.Section;
using RegistrationEntity = SchoolERP.Domain.Student.Registration.Entities.Registration;
using UserEntity = SchoolERP.Domain.Rbac.Entities.User;

namespace SchoolERP.Application.Features.Student.Commands.Registration.ApproveRegistration;

public class ApproveRegistrationCommandHandler : IRequestHandler<ApproveRegistrationCommand, Result<RegistrationResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public ApproveRegistrationCommandHandler(
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

    public async Task<Result<RegistrationResponse>> Handle(ApproveRegistrationCommand command, CancellationToken cancellationToken)
    {
        var branchId = _branchService.GetBranchId();
        var tenantId = _tenantService.GetTenantId();

        if (!branchId.HasValue || branchId.Value == Guid.Empty)
            return Error.Validation("Please select a branch to continue.");

        var entityResult = await _dbContext.GetEntityAsync<RegistrationEntity>(
            r => r.Id == command.Request.Id && r.BranchId == branchId.Value && !r.IsDeleted,
            "Registration", command.Request.Id.ToString(), cancellationToken);
        if (entityResult.IsFailure) return entityResult.Error;

        var reg = entityResult.Value;

        if (reg.Status != "under_review" && reg.Status != "document_verification" && reg.Status != "interview_scheduled")
            return Error.Conflict($"Cannot approve registration in '{reg.Status}' status.");

        if (reg.InterviewResult == "fail")
            return Error.Conflict("Cannot approve registration with failed interview.");

        var userId = _currentUserService.GetUserId() ?? 0;
        var oldStatus = reg.Status;

        reg.Status = "approved";
        reg.ApprovedAt = DateTime.UtcNow;
        reg.ApprovedBy = userId;
        reg.UpdatedAt = DateTime.UtcNow;

        _dbContext.Set<RbacAuditLog>().Add(new RbacAuditLog
        {
            TenantId = tenantId,
            PerformedBy = userId,
            Resource = "Registration",
            Action = "approve",
            OldValues = System.Text.Json.JsonSerializer.Serialize(new { Status = oldStatus }),
            NewValues = System.Text.Json.JsonSerializer.Serialize(new { Status = "approved" }),
            CreatedAt = DateTime.UtcNow
        });

        var response = reg.Adapt<RegistrationResponse>();
        return Result.Success(await FillComputedAsync(response, cancellationToken));
    }

    private async Task<RegistrationResponse> FillComputedAsync(RegistrationResponse r, CancellationToken ct)
    {
        var className = await _dbContext.Set<ClassEntity>().AsNoTracking().Where(c => c.Id == r.ClassId).Select(c => c.Name).FirstOrDefaultAsync(ct);
        string? sectionName = null;
        if (r.SectionId.HasValue)
            sectionName = await _dbContext.Set<SectionEntity>().AsNoTracking().Where(s => s.Id == r.SectionId.Value).Select(s => s.Name).FirstOrDefaultAsync(ct);
        string? approvedByName = null;
        if (r.ApprovedBy.HasValue)
            approvedByName = await _dbContext.Set<UserEntity>().AsNoTracking().Where(u => u.Id == r.ApprovedBy.Value).Select(u => u.Name).FirstOrDefaultAsync(ct);
        return r with { ClassName = className, SectionName = sectionName, ApprovedByName = approvedByName };
    }
}
