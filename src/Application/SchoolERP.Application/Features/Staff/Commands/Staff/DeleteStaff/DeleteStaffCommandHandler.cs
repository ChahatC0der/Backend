using MediatR;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Shared.Results;
using StaffEntity = SchoolERP.Domain.Staff.Entities.Staff;

namespace SchoolERP.Application.Features.Staff.Commands.Staff.DeleteStaff;

public class DeleteStaffCommandHandler : IRequestHandler<DeleteStaffCommand, Result<bool>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;

    public DeleteStaffCommandHandler(IApplicationDbContext dbContext, ICurrentBranchService branchService)
    {
        _dbContext = dbContext;
        _branchService = branchService;
    }

    public async Task<Result<bool>> Handle(DeleteStaffCommand command, CancellationToken cancellationToken)
    {
        var branchId = _branchService.GetBranchId() ?? Guid.Empty;

        var entityResult = await _dbContext.GetEntityAsync<StaffEntity>(
            s => s.Id == command.Id && s.BranchId == branchId && !s.IsDeleted,
            "Staff", command.Id.ToString(), cancellationToken);
        if (entityResult.IsFailure) return entityResult.Error;

        var staff = entityResult.Value;
        staff.IsDeleted = true;
        staff.DeletedAt = DateTime.UtcNow;
        staff.UpdatedAt = DateTime.UtcNow;

        return Result.Success(true);
    }
}
