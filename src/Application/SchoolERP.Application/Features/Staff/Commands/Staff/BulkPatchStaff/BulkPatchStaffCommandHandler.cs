using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Shared.Results;
using StaffEntity = SchoolERP.Domain.Staff.Entities.Staff;
using DepartmentEntity = SchoolERP.Domain.Staff.Entities.Department;

namespace SchoolERP.Application.Features.Staff.Commands.Staff.BulkPatchStaff;

public class BulkPatchStaffCommandHandler : IRequestHandler<BulkPatchStaffCommand, Result<bool>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;

    public BulkPatchStaffCommandHandler(IApplicationDbContext dbContext, ICurrentBranchService branchService)
    {
        _dbContext = dbContext;
        _branchService = branchService;
    }

    public async Task<Result<bool>> Handle(BulkPatchStaffCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var branchId = _branchService.GetBranchId() ?? Guid.Empty;

        if (request.DepartmentId.HasValue)
        {
            var deptExists = await _dbContext.EnsureEntityExistsAsync<DepartmentEntity>(request.DepartmentId.Value, cancellationToken);
            if (deptExists != null) return Error.NotFound("Department", request.DepartmentId.Value.ToString());
        }

        var entities = await _dbContext.Set<StaffEntity>()
            .Where(s => s.BranchId == branchId && request.Ids.Contains(s.Id) && !s.IsDeleted)
            .ToListAsync(cancellationToken);

        if (entities.Count != request.Ids.Count)
            return Error.NotFound("Staff", string.Join(",", request.Ids));

        foreach (var s in entities)
        {
            request.EmploymentType.PatchIfProvided(v => s.EmploymentType = v);
            request.StaffType.PatchIfProvided(v => s.StaffType = v);
            request.Designation.PatchIfProvided(v => s.Designation = v);
            if (request.DepartmentId.HasValue) s.DepartmentId = request.DepartmentId;
            if (!string.IsNullOrWhiteSpace(request.Status)) s.Status = request.Status;
            s.UpdatedAt = DateTime.UtcNow;
        }

        return Result.Success(true);
    }
}
