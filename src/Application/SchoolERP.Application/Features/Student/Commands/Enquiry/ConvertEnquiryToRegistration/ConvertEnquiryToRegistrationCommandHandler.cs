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

namespace SchoolERP.Application.Features.Student.Commands.Enquiry.ConvertEnquiryToRegistration;

public class ConvertEnquiryToRegistrationCommandHandler : IRequestHandler<ConvertEnquiryToRegistrationCommand, Result<EnquiryResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public ConvertEnquiryToRegistrationCommandHandler(
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

    public async Task<Result<EnquiryResponse>> Handle(ConvertEnquiryToRegistrationCommand command, CancellationToken cancellationToken)
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

        if (enquiry.Status == "converted")
            return Error.Conflict("Enquiry has already been converted.");

        var classErr = await _dbContext.EnsureEntityExistsAsync<ClassEntity>(request.ClassId, cancellationToken);
        if (classErr != null) return Error.NotFound("Class", request.ClassId.ToString());

        var userId = _currentUserService.GetUserId() ?? 0;
        var oldStatus = enquiry.Status;

        enquiry.Status = "converted";
        enquiry.UpdatedAt = DateTime.UtcNow;

        _dbContext.Set<EnquiryStatusLogEntity>().Add(new EnquiryStatusLogEntity
        {
            EnquiryId = enquiry.Id,
            OldStatus = oldStatus,
            NewStatus = "converted",
            Reason = "Converted to registration",
            PerformedBy = userId
        });

        // TODO: Create Registration entity — set enquiry.ConvertedToRegistrationId

        _dbContext.Set<RbacAuditLog>().Add(new RbacAuditLog
        {
            TenantId = tenantId,
            PerformedBy = userId,
            Resource = "Enquiry",
            Action = "convert",
            OldValues = System.Text.Json.JsonSerializer.Serialize(new { Status = oldStatus }),
            NewValues = System.Text.Json.JsonSerializer.Serialize(new { Status = "converted", request.AcademicYearId, request.ClassId, request.SectionId }),
            CreatedAt = DateTime.UtcNow
        });

        var response = enquiry.Adapt<EnquiryResponse>();

        string? className = null, assignedToName = null;
        if (response.ClassId.HasValue)
            className = await _dbContext.Set<ClassEntity>().AsNoTracking().Where(c => c.Id == response.ClassId.Value).Select(c => c.Name).FirstOrDefaultAsync(cancellationToken);
        if (response.AssignedTo.HasValue)
            assignedToName = await _dbContext.Set<UserEntity>().AsNoTracking().Where(u => u.Id == response.AssignedTo.Value).Select(u => u.Name).FirstOrDefaultAsync(cancellationToken);

        return Result.Success(response with { ClassName = className, AssignedToName = assignedToName });
    }
}
