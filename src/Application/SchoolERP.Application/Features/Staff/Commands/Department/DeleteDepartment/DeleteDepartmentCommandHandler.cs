using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Shared.Results;
using DepartmentEntity = SchoolERP.Domain.Staff.Entities.Department;

namespace SchoolERP.Application.Features.Staff.Commands.Department.DeleteDepartment;

public class DeleteDepartmentCommandHandler : IRequestHandler<DeleteDepartmentCommand, Result<bool>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;

    public DeleteDepartmentCommandHandler(IApplicationDbContext dbContext, ICurrentBranchService branchService)
    {
        _dbContext = dbContext;
        _branchService = branchService;
    }

    public async Task<Result<bool>> Handle(DeleteDepartmentCommand command, CancellationToken cancellationToken)
    {
        var branchId = _branchService.GetBranchId() ?? Guid.Empty;

        var entityResult = await _dbContext.GetEntityAsync<DepartmentEntity>(
            d => d.Id == command.Id && d.BranchId == branchId && !d.IsDeleted,
            "Department",
            command.Id.ToString(),
            cancellationToken);
        if (entityResult.IsFailure) return entityResult.Error;

        var department = entityResult.Value;
        department.IsDeleted = true;
        department.DeletedAt = DateTime.UtcNow;
        department.UpdatedAt = DateTime.UtcNow;

        return Result.Success(true);
    }
}
