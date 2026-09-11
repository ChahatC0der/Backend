using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Common.DTOs;
using SchoolERP.Application.Features.Staff.DTOs;
using SchoolERP.Domain.Shared.Results;
using DepartmentEntity = SchoolERP.Domain.Staff.Entities.Department;

namespace SchoolERP.Application.Features.Staff.Queries.Department.GetDepartments;

public class GetDepartmentsQueryHandler : IRequestHandler<GetDepartmentsQuery, Result<PagedResponse<DepartmentResponse>>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;

    public GetDepartmentsQueryHandler(IApplicationDbContext dbContext, ICurrentBranchService branchService)
    {
        _dbContext = dbContext;
        _branchService = branchService;
    }

    public async Task<Result<PagedResponse<DepartmentResponse>>> Handle(GetDepartmentsQuery query, CancellationToken cancellationToken)
    {
        var branchId = _branchService.GetBranchId() ?? Guid.Empty;
        var request = query.Request;

        var queryable = _dbContext.Set<DepartmentEntity>()
            .AsNoTracking()
            .Include(d => d.Hod)
            .Include(d => d.StaffMembers)
            .Where(d => d.BranchId == branchId && !d.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var search = request.SearchTerm.ToLower();
            queryable = queryable.Where(d => d.Name.ToLower().Contains(search) ||
                                             d.Code.ToLower().Contains(search));
        }

        queryable = request.SortBy?.ToLower() switch
        {
            "name" => request.SortOrder?.ToLower() == "desc" ? queryable.OrderByDescending(d => d.Name) : queryable.OrderBy(d => d.Name),
            "code" => request.SortOrder?.ToLower() == "desc" ? queryable.OrderByDescending(d => d.Code) : queryable.OrderBy(d => d.Code),
            _ => queryable.OrderBy(d => d.Name)
        };

        var totalCount = await queryable.CountAsync(cancellationToken);
        var items = await queryable
            .Skip((request.Page - 1) * request.Size)
            .Take(request.Size)
            .ToListAsync(cancellationToken);

        var data = items.Select(x => x.Adapt<DepartmentResponse>()).ToList();

        return Result.Success(new PagedResponse<DepartmentResponse>
        {
            Data = data,
            TotalCount = totalCount,
            Page = request.Page,
            Size = request.Size
        });
    }
}
