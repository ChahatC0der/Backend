using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Extensions;
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
        // 1️⃣ FETCH TENANT USING HELPER
        var tenantResult = await _dbContext.GetEntityByIdAsync<Tenant>(request.Id, cancellationToken);
        if (tenantResult.IsFailure)
            return tenantResult.Error;

        var tenant = tenantResult.Value;

        // 2️⃣ 🔥 MANUAL MAPPING (AVOID MAPSTER ISSUES)
        var response = new TenantResponse(
            tenant.Id,
            tenant.Code,
            tenant.Name,
            tenant.Subdomain,
            tenant.ContactEmail,
            tenant.Plan,
            tenant.Status,
            tenant.StudentCount,
            tenant.CreatedAt,
            tenant.UpdatedAt
        );

        return Result.Success(response);
    }
}