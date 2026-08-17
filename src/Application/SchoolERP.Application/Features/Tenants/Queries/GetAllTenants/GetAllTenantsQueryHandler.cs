using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.DTOs;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Tenants.DTOs;
using SchoolERP.Domain.Shared.Results;
using SchoolERP.Domain.Tenants.Entities;

namespace SchoolERP.Application.Features.Tenants.Queries.GetAllTenants;

public class GetAllTenantsQueryHandler : IRequestHandler<GetAllTenantsQuery, Result<PagedResponse<TenantResponse>>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetAllTenantsQueryHandler(IApplicationDbContext dbContext)
        => _dbContext = dbContext;

    public async Task<Result<PagedResponse<TenantResponse>>> Handle(GetAllTenantsQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.Set<Tenant>()
            .Where(t => !t.IsDeleted);

        // 🔥 1. SEARCH
        if (!string.IsNullOrEmpty(request.Request.SearchTerm))
        {
            var search = request.Request.SearchTerm.ToLower();
            query = query.Where(t =>
                t.Code.ToLower().Contains(search) ||
                t.Name.ToLower().Contains(search) ||
                t.Subdomain.ToLower().Contains(search) ||
                t.ContactEmail.ToLower().Contains(search));
        }

        // 🔥 2. SORTING
        query = request.Request.SortBy?.ToLower() switch
        {
            "code" => request.Request.SortOrder == "desc" ? query.OrderByDescending(t => t.Code) : query.OrderBy(t => t.Code),
            "name" => request.Request.SortOrder == "desc" ? query.OrderByDescending(t => t.Name) : query.OrderBy(t => t.Name),
            "status" => request.Request.SortOrder == "desc" ? query.OrderByDescending(t => t.Status) : query.OrderBy(t => t.Status),
            _ => request.Request.SortOrder == "desc" ? query.OrderByDescending(t => t.CreatedAt) : query.OrderBy(t => t.CreatedAt)
        };

        // 🔥 3. TOTAL COUNT
        var totalCount = await query.CountAsync(cancellationToken);

        // 🔥 4. PAGINATION
        var items = await query
            .Skip((request.Request.Page - 1) * request.Request.Size)
            .Take(request.Request.Size)
            .ToListAsync(cancellationToken);

        var response = new PagedResponse<TenantResponse>
        {
            Data = items.Adapt<IEnumerable<TenantResponse>>(),
            Page = request.Request.Page,
            Size = request.Request.Size,
            TotalCount = totalCount
        };

        return Result.Success(response);
    }
}