using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.DTOs;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Student.DTOs;
using SchoolERP.Domain.Shared.Results;
using FamilyEntity = SchoolERP.Domain.Student.Entities.Family;

namespace SchoolERP.Application.Features.Student.Queries.Family.GetFamilies;

public class GetFamiliesQueryHandler : IRequestHandler<GetFamiliesQuery, Result<PagedResponse<FamilyResponse>>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;

    public GetFamiliesQueryHandler(IApplicationDbContext dbContext, ICurrentBranchService branchService)
    {
        _dbContext = dbContext;
        _branchService = branchService;
    }

    public async Task<Result<PagedResponse<FamilyResponse>>> Handle(GetFamiliesQuery query, CancellationToken cancellationToken)
    {
        var branchId = _branchService.GetBranchId();
        if (branchId == null) return Error.Conflict("Please Choose a Branch");

        var request = query.Request;

        var q = _dbContext.Set<FamilyEntity>()
            .AsNoTracking()
            .Include(f => f.Students)
            .Where(f => f.BranchId == branchId && !f.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var s = request.SearchTerm.ToLower();
            q = q.Where(f =>
                (f.HeadName != null && f.HeadName.ToLower().Contains(s)) ||
                (f.HeadMobile != null && f.HeadMobile.Contains(s)));
        }

        if (!string.IsNullOrWhiteSpace(request.HeadMobile))
            q = q.Where(f => f.HeadMobile == request.HeadMobile);

        q = request.SortBy?.ToLower() switch
        {
            "headname" => request.SortOrder?.ToLower() == "desc" ? q.OrderByDescending(f => f.HeadName) : q.OrderBy(f => f.HeadName),
            "studentcount" => request.SortOrder?.ToLower() == "desc" ? q.OrderByDescending(f => f.Students.Count) : q.OrderBy(f => f.Students.Count),
            _ => q.OrderByDescending(f => f.CreatedAt)
        };

        var totalCount = await q.CountAsync(cancellationToken);
        var items = await q.Skip((request.Page - 1) * request.Size).Take(request.Size).ToListAsync(cancellationToken);

        var data = items.Select(f => f.Adapt<FamilyResponse>()).ToList();

        return Result.Success(new PagedResponse<FamilyResponse>
        {
            Data = data,
            TotalCount = totalCount,
            Page = request.Page,
            Size = request.Size
        });
    }
}
