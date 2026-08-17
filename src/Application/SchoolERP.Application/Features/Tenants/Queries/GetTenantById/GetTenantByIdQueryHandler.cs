using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Tenants.DTOs;
using SchoolERP.Domain.Shared.Results;
using SchoolERP.Domain.Tenants.Entities;

namespace SchoolERP.Application.Features.Tenants.Queries.GetTenantById;

public class GetTenantByIdQueryHandler : IRequestHandler<GetTenantByIdQuery, Result<TenantResponse>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetTenantByIdQueryHandler(IApplicationDbContext dbContext)
        => _dbContext = dbContext;

    public async Task<Result<TenantResponse>> Handle(GetTenantByIdQuery request, CancellationToken cancellationToken)
    {
        var tenant = await _dbContext.Set<Tenant>()
            .FirstOrDefaultAsync(t => t.Id == request.Id && !t.IsDeleted, cancellationToken);

        if (tenant == null)
            return Error.NotFound("Tenant", request.Id.ToString());

        return Result.Success(tenant.Adapt<TenantResponse>());
    }
}