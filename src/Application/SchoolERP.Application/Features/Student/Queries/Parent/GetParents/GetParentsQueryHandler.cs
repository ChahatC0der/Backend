using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Common.DTOs;
using SchoolERP.Application.Features.Student.DTOs;
using SchoolERP.Domain.Shared.Results;
using ParentEntity = SchoolERP.Domain.Student.Entities.Parent;
using StudentEntity = SchoolERP.Domain.Student.Entities.Student;

namespace SchoolERP.Application.Features.Student.Queries.Parent.GetParents;

public class GetParentsQueryHandler : IRequestHandler<GetParentsQuery, Result<PagedResponse<ParentResponse>>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;

    public GetParentsQueryHandler(IApplicationDbContext dbContext, ICurrentBranchService branchService)
    {
        _dbContext = dbContext;
        _branchService = branchService;
    }

    public async Task<Result<PagedResponse<ParentResponse>>> Handle(GetParentsQuery query, CancellationToken cancellationToken)
    {
        var branchId = _branchService.GetBranchId() ?? Guid.Empty;
        var request = query.Request;

        var queryable = from p in _dbContext.Set<ParentEntity>()
                        join s in _dbContext.Set<StudentEntity>() on p.StudentId equals s.Id
                        where !p.IsDeleted && !s.IsDeleted && s.BranchId == branchId
                        select p;

        if (query.StudentId.HasValue)
            queryable = queryable.Where(p => p.StudentId == query.StudentId.Value);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var search = request.SearchTerm.ToLower();
            queryable = queryable.Where(p =>
                p.FirstName.ToLower().Contains(search) ||
                (p.LastName != null && p.LastName.ToLower().Contains(search)) ||
                (p.Mobile != null && p.Mobile.Contains(search)) ||
                (p.Email != null && p.Email.ToLower().Contains(search)));
        }

        queryable = request.SortBy?.ToLower() switch
        {
            "firstname" => request.SortOrder?.ToLower() == "desc" ? queryable.OrderByDescending(p => p.FirstName) : queryable.OrderBy(p => p.FirstName),
            _ => queryable.OrderBy(p => p.FirstName)
        };

        var totalCount = await queryable.CountAsync(cancellationToken);
        var items = await queryable
            .Skip((request.Page - 1) * request.Size)
            .Take(request.Size)
            .ToListAsync(cancellationToken);

        var data = items.Select(x => x.Adapt<ParentResponse>()).ToList();

        return Result.Success(new PagedResponse<ParentResponse>
        {
            Data = data,
            TotalCount = totalCount,
            Page = request.Page,
            Size = request.Size
        });
    }
}
