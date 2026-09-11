using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Staff.DTOs;
using SchoolERP.Domain.Shared.Results;
using DepartmentEntity = SchoolERP.Domain.Staff.Entities.Department;

namespace SchoolERP.Application.Features.Staff.Queries.Department.GetDepartmentsLight;

public class GetDepartmentsLightQueryHandler : IRequestHandler<GetDepartmentsLightQuery, Result<List<DepartmentLightResponse>>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;

    public GetDepartmentsLightQueryHandler(IApplicationDbContext dbContext, ICurrentBranchService branchService)
    {
        _dbContext = dbContext;
        _branchService = branchService;
    }

    public async Task<Result<List<DepartmentLightResponse>>> Handle(GetDepartmentsLightQuery query, CancellationToken cancellationToken)
    {
        var branchId = _branchService.GetBranchId() ?? Guid.Empty;

        var items = await _dbContext.Set<DepartmentEntity>()
            .AsNoTracking()
            .Where(d => d.BranchId == branchId && !d.IsDeleted)
            .OrderBy(d => d.Name)
            .ProjectToType<DepartmentLightResponse>()
            .ToListAsync(cancellationToken);

        return Result.Success(items);
    }
}
