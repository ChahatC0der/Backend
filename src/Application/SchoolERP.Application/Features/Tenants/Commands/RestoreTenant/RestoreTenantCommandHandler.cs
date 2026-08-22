using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Shared.Results;
using SchoolERP.Domain.Tenants.Entities;
using System.Linq.Expressions;

namespace SchoolERP.Application.Features.Tenants.Commands.RestoreTenant;

public class RestoreTenantCommandHandler : IRequestHandler<RestoreTenantCommand, Result<bool>>
{
    private readonly IApplicationDbContext _dbContext;

    public RestoreTenantCommandHandler(IApplicationDbContext dbContext)
        => _dbContext = dbContext;

    public async Task<Result<bool>> Handle(RestoreTenantCommand request, CancellationToken cancellationToken)
    {
        // 1️⃣ FETCH DELETED TENANT (Using custom predicate)
        var tenantResult = await _dbContext.GetEntityAsync<Tenant>(
            t => t.Id == request.Id && t.IsDeleted, // 🔥 Sirf deleted wala
            nameof(Tenant),
            request.Id.ToString(),
            cancellationToken);

        if (tenantResult.IsFailure)
            return tenantResult.Error;

        var tenant = tenantResult.Value;

        // 2️⃣ 🔥 UNIQUENESS CHECKS (RESTORE SE PEHLE)
        var checks = new (Expression<Func<Tenant, bool>> Predicate, string Message)[]
        {
            (t => t.Code == tenant.Code && t.Id != request.Id && !t.IsDeleted,
                $"Code '{tenant.Code}' is already used by another active tenant. Cannot restore."),

            (t => t.Subdomain == tenant.Subdomain && t.Id != request.Id && !t.IsDeleted,
                $"Subdomain '{tenant.Subdomain}' is already used by another active tenant. Cannot restore."),

            (t => t.ContactEmail == tenant.ContactEmail && t.Id != request.Id && !t.IsDeleted,
                $"Email '{tenant.ContactEmail}' is already used by another active tenant. Cannot restore.")
        };

        var error = await _dbContext.EnsureAllUniqueAsync(checks, cancellationToken);
        if (error is not null)
            return error;

        // 3️⃣ RESTORE
        tenant.IsDeleted = false;
        tenant.DeletedAt = null;

        // 🔥 UpdatedAt auto-set by SaveChangesAsync override (no manual set needed)

        // 4️⃣ SAVE
        await _dbContext.SaveChangesAsync(cancellationToken);

        // 5️⃣ RETURN
        return Result.Success(true, $"Tenant '{tenant.Name}' restored successfully.");
    }
}