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
using UserEntity = SchoolERP.Domain.Rbac.Entities.User;

namespace SchoolERP.Application.Features.Student.Commands.Enquiry.CreateEnquiry;

public class CreateEnquiryCommandHandler : IRequestHandler<CreateEnquiryCommand, Result<EnquiryResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public CreateEnquiryCommandHandler(
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

    public async Task<Result<EnquiryResponse>> Handle(CreateEnquiryCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var branchId = _branchService.GetBranchId();
        var tenantId = _tenantService.GetTenantId();

        if (!branchId.HasValue || branchId.Value == Guid.Empty)
            return Error.Validation("Please select a branch to continue.");

        if (request.ClassId.HasValue)
        {
            var classErr = await _dbContext.EnsureEntityExistsAsync<ClassEntity>(request.ClassId.Value, cancellationToken);
            if (classErr != null) return Error.NotFound("Class", request.ClassId.Value.ToString());
        }

        if (request.AssignedTo.HasValue)
        {
            var userErr = await _dbContext.EnsureEntityExistsAsync<UserEntity>(request.AssignedTo.Value, cancellationToken);
            if (userErr != null) return Error.NotFound("User", request.AssignedTo.Value.ToString());
        }

        var enquiry = request.Adapt<EnquiryEntity>();
        enquiry.EnquiryDate = DateTime.UtcNow;
        enquiry.Status = "new";

        _dbContext.Set<EnquiryEntity>().Add(enquiry);

        var userId = _currentUserService.GetUserId() ?? 0;

        //_dbContext.Set<RbacAuditLog>().Add(new RbacAuditLog
        //{
        //    TenantId = tenantId,
        //    PerformedBy = userId,
        //    Resource = "Enquiry",
        //    Action = "create",
        //    NewValues = System.Text.Json.JsonSerializer.Serialize(new { enquiry.ParentName, enquiry.Mobile, enquiry.Source, enquiry.ClassId }),
        //    CreatedAt = DateTime.UtcNow
        //});

        var response = enquiry.Adapt<EnquiryResponse>();
        return Result.Success(await FillComputedAsync(response, cancellationToken));
    }

    private async Task<EnquiryResponse> FillComputedAsync(EnquiryResponse r, CancellationToken ct)
    {
        string? className = null;
        string? assignedToName = null;

        if (r.ClassId.HasValue)
            className = await _dbContext.Set<ClassEntity>().AsNoTracking()
                .Where(c => c.Id == r.ClassId.Value).Select(c => c.Name).FirstOrDefaultAsync(ct);

        if (r.AssignedTo.HasValue)
            assignedToName = await _dbContext.Set<UserEntity>().AsNoTracking()
                .Where(u => u.Id == r.AssignedTo.Value).Select(u => u.Name).FirstOrDefaultAsync(ct);

        return r with { ClassName = className, AssignedToName = assignedToName };
    }
}
