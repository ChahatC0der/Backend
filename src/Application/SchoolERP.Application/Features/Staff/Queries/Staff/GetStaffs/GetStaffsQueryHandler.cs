using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Common.DTOs;
using SchoolERP.Application.Features.Staff.DTOs;
using SchoolERP.Domain.Shared.Results;
using StaffEntity = SchoolERP.Domain.Staff.Entities.Staff;

namespace SchoolERP.Application.Features.Staff.Queries.Staff.GetStaffs;

public class GetStaffsQueryHandler : IRequestHandler<GetStaffsQuery, Result<PagedResponse<StaffResponse>>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;

    public GetStaffsQueryHandler(IApplicationDbContext dbContext, ICurrentBranchService branchService)
    {
        _dbContext = dbContext;
        _branchService = branchService;
    }

    public async Task<Result<PagedResponse<StaffResponse>>> Handle(GetStaffsQuery query, CancellationToken cancellationToken)
    {
        var branchId = _branchService.GetBranchId() ?? Guid.Empty;
        var request = query.Request;

        var queryable = _dbContext.Set<StaffEntity>()
            .AsNoTracking()
            .Include(s => s.Department)
            .Where(s => s.BranchId == branchId && !s.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var search = request.SearchTerm.ToLower();
            queryable = queryable.Where(s =>
                s.FirstName.ToLower().Contains(search) ||
                (s.LastName != null && s.LastName.ToLower().Contains(search)) ||
                s.StaffId.ToLower().Contains(search) ||
                s.Designation.ToLower().Contains(search) ||
                (s.PersonalMobile != null && s.PersonalMobile.Contains(search)));
        }

        queryable = request.SortBy?.ToLower() switch
        {
            "firstname" => request.SortOrder?.ToLower() == "desc" ? queryable.OrderByDescending(s => s.FirstName) : queryable.OrderBy(s => s.FirstName),
            "staffid" => request.SortOrder?.ToLower() == "desc" ? queryable.OrderByDescending(s => s.StaffId) : queryable.OrderBy(s => s.StaffId),
            "designation" => request.SortOrder?.ToLower() == "desc" ? queryable.OrderByDescending(s => s.Designation) : queryable.OrderBy(s => s.Designation),
            "joiningdate" => request.SortOrder?.ToLower() == "desc" ? queryable.OrderByDescending(s => s.JoiningDate) : queryable.OrderBy(s => s.JoiningDate),
            _ => queryable.OrderBy(s => s.FirstName)
        };

        var totalCount = await queryable.CountAsync(cancellationToken);
        var items = await queryable
            .Skip((request.Page - 1) * request.Size)
            .Take(request.Size)
            .ToListAsync(cancellationToken);

        var data = items.Select(x => x.Adapt<StaffResponse>()).ToList();

        return Result.Success(new PagedResponse<StaffResponse>
        {
            Data = data,
            TotalCount = totalCount,
            Page = request.Page,
            Size = request.Size
        });
    }
}
