using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Shared.Results;
using DepartmentEntity = SchoolERP.Domain.Staff.Entities.Department;

namespace SchoolERP.Application.Features.Staff.Commands.Department.RestoreDepartment;

public class RestoreDepartmentCommandHandler : IRequestHandler<RestoreDepartmentCommand, Result<bool>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;

    public RestoreDepartmentCommandHandler(IApplicationDbContext dbContext, ICurrentBranchService branchService)
    {
        _dbContext = dbContext;
        _branchService = branchService;
    }

    public async Task<Result<bool>> Handle(RestoreDepartmentCommand command, CancellationToken cancellationToken)
    {
        var branchId = _branchService.GetBranchId() ?? Guid.Empty;

        var department = await _dbContext.Set<DepartmentEntity>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(d => d.Id == command.Id && d.BranchId == branchId && d.IsDeleted, cancellationToken);

        if (department == null)
            return Error.NotFound("Department", command.Id.ToString());

        department.IsDeleted = false;
        department.DeletedAt = null;
        department.UpdatedAt = DateTime.UtcNow;

        return Result.Success(true);
    }
}
