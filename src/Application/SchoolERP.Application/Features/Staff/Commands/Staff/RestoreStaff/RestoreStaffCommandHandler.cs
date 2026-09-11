using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Shared.Results;
using StaffEntity = SchoolERP.Domain.Staff.Entities.Staff;

namespace SchoolERP.Application.Features.Staff.Commands.Staff.RestoreStaff;

public class RestoreStaffCommandHandler : IRequestHandler<RestoreStaffCommand, Result<bool>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;

    public RestoreStaffCommandHandler(IApplicationDbContext dbContext, ICurrentBranchService branchService)
    {
        _dbContext = dbContext;
        _branchService = branchService;
    }

    public async Task<Result<bool>> Handle(RestoreStaffCommand command, CancellationToken cancellationToken)
    {
        var branchId = _branchService.GetBranchId() ?? Guid.Empty;

        var staff = await _dbContext.Set<StaffEntity>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.Id == command.Id && s.BranchId == branchId && s.IsDeleted, cancellationToken);
        if (staff == null)
            return Error.NotFound("Staff", command.Id.ToString());

        staff.IsDeleted = false;
        staff.DeletedAt = null;
        staff.UpdatedAt = DateTime.UtcNow;

        return Result.Success(true);
    }
}
