using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.DTOs;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Branches.DTOs;
using SchoolERP.Domain.Shared.Results;
using SchoolERP.Domain.Tenants.Entities;

namespace SchoolERP.Application.Features.Branches.Queries.GetBranches;

public class GetBranchesQueryHandler : IRequestHandler<GetBranchesQuery, Result<PagedResponse<BranchResponse>>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentTenantService _tenantService;

    public GetBranchesQueryHandler(
        IApplicationDbContext dbContext,
        ICurrentTenantService tenantService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
    }

    public async Task<Result<PagedResponse<BranchResponse>>> Handle(
        GetBranchesQuery request,
        CancellationToken cancellationToken)
    {
        var tenantId = _tenantService.GetTenantId();

        var query = _dbContext.Set<Branch>()
            .Where(b => b.TenantId == tenantId && !b.IsDeleted);

        // Search
        if (!string.IsNullOrEmpty(request.Request.SearchTerm))
        {
            var search = request.Request.SearchTerm.ToLower();

            query = query.Where(b =>
                b.Name.ToLower().Contains(search) ||
                b.Code.ToLower().Contains(search) ||
                (b.ContactPerson != null && b.ContactPerson.ToLower().Contains(search)));
        }

        // Sorting
        query = request.Request.SortBy?.ToLower() switch
        {
            "name" => request.Request.SortOrder == "desc"
                ? query.OrderByDescending(b => b.Name)
                : query.OrderBy(b => b.Name),

            "code" => request.Request.SortOrder == "desc"
                ? query.OrderByDescending(b => b.Code)
                : query.OrderBy(b => b.Code),

            "status" => request.Request.SortOrder == "desc"
                ? query.OrderByDescending(b => b.Status)
                : query.OrderBy(b => b.Status),

            _ => request.Request.SortOrder == "desc"
                ? query.OrderByDescending(b => b.CreatedAt)
                : query.OrderBy(b => b.CreatedAt)
        };

        var totalCount = await query.CountAsync(cancellationToken);

        // MANUAL PROJECTION
        var items = await query
            .Skip((request.Request.Page - 1) * request.Request.Size)
            .Take(request.Request.Size)
            .Select(b => new BranchResponse(
                b.Id,
                b.TenantId,
                b.Name,
                b.Code,
                b.Address,
                b.Phone,
                b.Email,
                b.ContactPerson,
                b.IsDefault,
                b.Status,
                b.CreatedAt,
                b.UpdatedAt
            ))
            .ToListAsync(cancellationToken);

        var response = new PagedResponse<BranchResponse>
        {
            Data = items,
            Page = request.Request.Page,
            Size = request.Request.Size,
            TotalCount = totalCount
        };

        return Result.Success(response);
    }
}