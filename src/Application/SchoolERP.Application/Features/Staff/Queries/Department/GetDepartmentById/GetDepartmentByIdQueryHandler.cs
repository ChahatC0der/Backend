using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Staff.DTOs;
using SchoolERP.Domain.Shared.Results;
using DepartmentEntity = SchoolERP.Domain.Staff.Entities.Department;

namespace SchoolERP.Application.Features.Staff.Queries.Department.GetDepartmentById;

public class GetDepartmentByIdQueryHandler : IRequestHandler<GetDepartmentByIdQuery, Result<DepartmentResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;

    public GetDepartmentByIdQueryHandler(IApplicationDbContext dbContext, ICurrentBranchService branchService)
    {
        _dbContext = dbContext;
        _branchService = branchService;
    }

    public async Task<Result<DepartmentResponse>> Handle(GetDepartmentByIdQuery query, CancellationToken cancellationToken)
    {
        var branchId = _branchService.GetBranchId() ?? Guid.Empty;

        var department = await _dbContext.Set<DepartmentEntity>()
            .AsNoTracking()
            .Include(d => d.Hod)
            .Include(d => d.StaffMembers)
            .FirstOrDefaultAsync(d => d.Id == query.Id && d.BranchId == branchId && !d.IsDeleted, cancellationToken);

        if (department == null)
            return Error.NotFound("Department", query.Id.ToString());

        return Result.Success(department.Adapt<DepartmentResponse>());
    }
}
