using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Rbac.Entities;
using SchoolERP.Domain.Shared.Results;
using EnquiryEntity = SchoolERP.Domain.Student.Enquiry.Entities.Enquiry;

namespace SchoolERP.Application.Features.Student.Commands.Enquiry.RestoreEnquiry;

public class RestoreEnquiryCommandHandler : IRequestHandler<RestoreEnquiryCommand, Result<bool>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public RestoreEnquiryCommandHandler(
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

    public async Task<Result<bool>> Handle(RestoreEnquiryCommand command, CancellationToken cancellationToken)
    {
        var branchId = _branchService.GetBranchId();
        var tenantId = _tenantService.GetTenantId();

        if (!branchId.HasValue || branchId.Value == Guid.Empty)
            return Error.Validation("Please select a branch to continue.");

        var enquiry = await _dbContext.Set<EnquiryEntity>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(e => e.Id == command.Id && e.BranchId == branchId.Value && e.IsDeleted, cancellationToken);

        if (enquiry == null)
            return Error.NotFound("Enquiry", command.Id.ToString());

        enquiry.IsDeleted = false;
        enquiry.DeletedAt = null;
        enquiry.UpdatedAt = DateTime.UtcNow;

        var userId = _currentUserService.GetUserId() ?? 0;
        //_dbContext.Set<RbacAuditLog>().Add(new RbacAuditLog
        //{
        //    TenantId = tenantId,
        //    PerformedBy = userId,
        //    Resource = "Enquiry",
        //    Action = "restore",
        //    NewValues = System.Text.Json.JsonSerializer.Serialize(new { enquiry.Id, enquiry.ParentName }),
        //    CreatedAt = DateTime.UtcNow
        //});

        return Result.Success(true);
    }
}
