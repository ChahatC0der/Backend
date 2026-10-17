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

namespace SchoolERP.Application.Features.Student.Commands.Registration.RecordInterviewResult;

public class RecordInterviewResultCommandHandler : IRequestHandler<RecordInterviewResultCommand, Result<RegistrationResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public RecordInterviewResultCommandHandler(
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

    public async Task<Result<RegistrationResponse>> Handle(RecordInterviewResultCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var branchId = _branchService.GetBranchId();
        var tenantId = _tenantService.GetTenantId();

        if (!branchId.HasValue || branchId.Value == Guid.Empty)
            return Error.Validation("Please select a branch to continue.");

        var entityResult = await _dbContext.GetEntityAsync<RegistrationEntity>(
            r => r.Id == request.Id && r.BranchId == branchId.Value && !r.IsDeleted,
            "Registration", request.Id.ToString(), cancellationToken);
        if (entityResult.IsFailure) return entityResult.Error;

        var reg = entityResult.Value;

        if (reg.Status != "interview_scheduled")
            return Error.Conflict($"Cannot record result in '{reg.Status}' status.");

        reg.InterviewResult = request.InterviewResult;
        reg.InterviewFeedback = request.InterviewFeedback;
        reg.UpdatedAt = DateTime.UtcNow;

        var userId = _currentUserService.GetUserId() ?? 0;
        //_dbContext.Set<RbacAuditLog>().Add(new RbacAuditLog
        //{
        //    TenantId = tenantId,
        //    PerformedBy = userId,
        //    Resource = "Registration",
        //    Action = "record_interview_result",
        //    NewValues = System.Text.Json.JsonSerializer.Serialize(new { request.InterviewResult, request.InterviewFeedback }),
        //    CreatedAt = DateTime.UtcNow
        //});

        var response = reg.Adapt<RegistrationResponse>();
        return Result.Success(await FillComputedAsync(response, cancellationToken));
    }

    private async Task<RegistrationResponse> FillComputedAsync(RegistrationResponse r, CancellationToken ct)
    {
        var className = await _dbContext.Set<ClassEntity>().AsNoTracking().Where(c => c.Id == r.ClassId).Select(c => c.Name).FirstOrDefaultAsync(ct);
        string? sectionName = null;
        if (r.SectionId.HasValue)
            sectionName = await _dbContext.Set<SectionEntity>().AsNoTracking().Where(s => s.Id == r.SectionId.Value).Select(s => s.Name).FirstOrDefaultAsync(ct);
        string? interviewerName = null;
        if (r.InterviewerId.HasValue)
            interviewerName = await _dbContext.Set<UserEntity>().AsNoTracking().Where(u => u.Id == r.InterviewerId.Value).Select(u => u.Name).FirstOrDefaultAsync(ct);
        return r with { ClassName = className, SectionName = sectionName, InterviewerName = interviewerName };
    }
}
