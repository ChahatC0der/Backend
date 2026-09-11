using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Shared.Results;
using DepartmentEntity = SchoolERP.Domain.Staff.Entities.Department;

namespace SchoolERP.Application.Features.Staff.Commands.Department.BulkDeleteDepartment;

public class BulkDeleteDepartmentCommandHandler : IRequestHandler<BulkDeleteDepartmentCommand, Result<bool>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;

    public BulkDeleteDepartmentCommandHandler(IApplicationDbContext dbContext, ICurrentBranchService branchService)
    {
        _dbContext = dbContext;
        _branchService = branchService;
    }

    public async Task<Result<bool>> Handle(BulkDeleteDepartmentCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var branchId = _branchService.GetBranchId() ?? Guid.Empty;

        var entities = await _dbContext.Set<DepartmentEntity>()
            .Where(d => d.BranchId == branchId && request.Ids.Contains(d.Id) && !d.IsDeleted)
            .ToListAsync(cancellationToken);

        if (entities.Count != request.Ids.Count)
            return Error.NotFound("Department", string.Join(",", request.Ids));

        foreach (var dept in entities)
        {
            dept.IsDeleted = true;
            dept.DeletedAt = DateTime.UtcNow;
            dept.UpdatedAt = DateTime.UtcNow;
        }

        return Result.Success(true);
    }
}
