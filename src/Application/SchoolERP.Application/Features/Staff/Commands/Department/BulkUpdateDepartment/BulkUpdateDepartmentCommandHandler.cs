using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Shared.Results;
using DepartmentEntity = SchoolERP.Domain.Staff.Entities.Department;
using StaffEntity = SchoolERP.Domain.Staff.Entities.Staff;

namespace SchoolERP.Application.Features.Staff.Commands.Department.BulkUpdateDepartment;

public class BulkUpdateDepartmentCommandHandler : IRequestHandler<BulkUpdateDepartmentCommand, Result<bool>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;

    public BulkUpdateDepartmentCommandHandler(IApplicationDbContext dbContext, ICurrentBranchService branchService)
    {
        _dbContext = dbContext;
        _branchService = branchService;
    }

    public async Task<Result<bool>> Handle(BulkUpdateDepartmentCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var branchId = _branchService.GetBranchId() ?? Guid.Empty;

        var entities = await _dbContext.Set<DepartmentEntity>()
            .Where(d => d.BranchId == branchId && request.Ids.Contains(d.Id) && !d.IsDeleted)
            .ToListAsync(cancellationToken);

        if (entities.Count != request.Ids.Count)
            return Error.NotFound("Department", string.Join(",", request.Ids));

        if (request.HodId.HasValue)
        {
            var hodExists = await _dbContext.EnsureEntityExistsAsync<StaffEntity>(request.HodId.Value, cancellationToken);
            if (hodExists != null) return Error.NotFound("Staff", request.HodId.Value.ToString());
        }

        var outsideConflict = await _dbContext.EnsureUniqueAsync<DepartmentEntity>(
            d => d.BranchId == branchId && d.Code == request.Code && !request.Ids.Contains(d.Id) && !d.IsDeleted,
            $"Another department with code '{request.Code}' already exists.",
            cancellationToken);
        if (outsideConflict != null) return outsideConflict;

        foreach (var dept in entities)
        {
            dept.Name = request.Name;
            dept.Code = request.Code;
            dept.HodId = request.HodId;
            dept.UpdatedAt = DateTime.UtcNow;
        }

        return Result.Success(true);
    }
}
