using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Shared.Results;
using DepartmentEntity = SchoolERP.Domain.Staff.Entities.Department;
using StaffEntity = SchoolERP.Domain.Staff.Entities.Staff;

namespace SchoolERP.Application.Features.Staff.Commands.Department.BulkPatchDepartment;

public class BulkPatchDepartmentCommandHandler : IRequestHandler<BulkPatchDepartmentCommand, Result<bool>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;

    public BulkPatchDepartmentCommandHandler(IApplicationDbContext dbContext, ICurrentBranchService branchService)
    {
        _dbContext = dbContext;
        _branchService = branchService;
    }

    public async Task<Result<bool>> Handle(BulkPatchDepartmentCommand command, CancellationToken cancellationToken)
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

        foreach (var dept in entities)
        {
            request.Name.PatchIfProvided(value => dept.Name = value);
            request.Code.PatchIfProvided(value => dept.Code = value);
            if (request.HodId.HasValue) dept.HodId = request.HodId;

            if (!string.IsNullOrWhiteSpace(request.Code))
            {
                var conflict = await _dbContext.EnsureUniqueAsync<DepartmentEntity>(
                    d => d.BranchId == branchId && d.Code == dept.Code && d.Id != dept.Id && !d.IsDeleted,
                    $"Another department with code '{dept.Code}' already exists.",
                    cancellationToken);
                if (conflict != null) return conflict;
            }

            dept.UpdatedAt = DateTime.UtcNow;
        }

        return Result.Success(true);
    }
}
