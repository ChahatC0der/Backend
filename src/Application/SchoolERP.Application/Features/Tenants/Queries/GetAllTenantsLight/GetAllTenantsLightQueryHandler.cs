using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Tenants.DTOs;
using SchoolERP.Domain.Shared.Results;
using SchoolERP.Domain.Tenants.Entities;

namespace SchoolERP.Application.Features.Tenants.Queries.GetAllTenantsLight;

public class GetAllTenantsLightQueryHandler : IRequestHandler<GetAllTenantsLightQuery, Result<IEnumerable<TenantLightResponse>>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetAllTenantsLightQueryHandler(IApplicationDbContext dbContext)
        => _dbContext = dbContext;

    public async Task<Result<IEnumerable<TenantLightResponse>>> Handle(GetAllTenantsLightQuery request, CancellationToken cancellationToken)
    {
        var tenants = await _dbContext.Set<Tenant>()
            .Where(t => !t.IsDeleted)
            .OrderBy(t => t.Name)
            .Select(t => new TenantLightResponse(t.Id, t.Code, t.Name))
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<TenantLightResponse>>(tenants);
    }
}