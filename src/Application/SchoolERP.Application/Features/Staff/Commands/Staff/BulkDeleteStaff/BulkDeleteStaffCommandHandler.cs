using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Shared.Results;
using StaffEntity = SchoolERP.Domain.Staff.Entities.Staff;

namespace SchoolERP.Application.Features.Staff.Commands.Staff.BulkDeleteStaff;

public class BulkDeleteStaffCommandHandler : IRequestHandler<BulkDeleteStaffCommand, Result<bool>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;

    public BulkDeleteStaffCommandHandler(IApplicationDbContext dbContext, ICurrentBranchService branchService)
    {
        _dbContext = dbContext;
        _branchService = branchService;
    }

    public async Task<Result<bool>> Handle(BulkDeleteStaffCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var branchId = _branchService.GetBranchId() ?? Guid.Empty;

        var entities = await _dbContext.Set<StaffEntity>()
            .Where(s => s.BranchId == branchId && request.Ids.Contains(s.Id) && !s.IsDeleted)
            .ToListAsync(cancellationToken);

        if (entities.Count != request.Ids.Count)
            return Error.NotFound("Staff", string.Join(",", request.Ids));

        foreach (var s in entities)
        {
            s.IsDeleted = true;
            s.DeletedAt = DateTime.UtcNow;
            s.UpdatedAt = DateTime.UtcNow;
        }

        return Result.Success(true);
    }
}
