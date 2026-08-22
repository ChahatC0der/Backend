using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Tenants.DTOs;
using SchoolERP.Domain.Shared.Results;
using SchoolERP.Domain.Tenants.Entities;
using System.Linq.Expressions;

namespace SchoolERP.Application.Features.Tenants.Commands.UpdateTenant;

public class UpdateTenantCommandHandler : IRequestHandler<UpdateTenantCommand, Result<TenantResponse>>
{
    private readonly IApplicationDbContext _dbContext;

    public UpdateTenantCommandHandler(IApplicationDbContext dbContext)
        => _dbContext = dbContext;

    public async Task<Result<TenantResponse>> Handle(UpdateTenantCommand request, CancellationToken cancellationToken)
    {
        // 1️⃣ 🔥 FETCH TENANT USING HELPER (No manual null check)
        var tenantResult = await _dbContext.GetEntityByIdAsync<Tenant>(request.Id, cancellationToken);
        Console.WriteLine(tenantResult);
        if (tenantResult.IsFailure)
            return tenantResult.Error;

        var tenant = tenantResult.Value;

        // 2️⃣ PREPARE CLEAN VALUES
        var cleanSubdomain = request.Request.Subdomain.Trim().ToLower().Replace(" ", "-");
        var code = request.Request.Code.Trim().ToUpper();
        var email = request.Request.ContactEmail.Trim().ToLower();

        // 3️⃣ 🔥 UNIQUENESS CHECKS (EXCLUDE CURRENT TENANT)
        var checks = new (Expression<Func<Tenant, bool>> Predicate, string Message)[]
        {
            (t => t.Code == code && t.Id != request.Id && !t.IsDeleted,
                $"Code '{code}' is already taken by another tenant."),

            (t => t.Name == request.Request.Name && t.Id != request.Id && !t.IsDeleted,
                $"Name '{request.Request.Name}' is already taken by another tenant."),

            (t => t.Subdomain == cleanSubdomain && t.Id != request.Id && !t.IsDeleted,
                $"Subdomain '{cleanSubdomain}' is already taken by another tenant."),

            (t => t.ContactEmail == email && t.Id != request.Id && !t.IsDeleted,
                $"Email '{email}' is already registered by another tenant.")
        };

        var error = await _dbContext.EnsureAllUniqueAsync(checks, cancellationToken);
        if (error is not null)
            return error;

        // 4️⃣ 🔥 UPDATE USING MAPSTER (EXISTING OBJECT)
        request.Request.Adapt(tenant);
        tenant.Subdomain = cleanSubdomain;

        // 5️⃣ SAVE (UpdatedAt auto-set in SaveChangesAsync)
        await _dbContext.SaveChangesAsync(cancellationToken);


        // 6️⃣ RETURN RESPONSE
        return Result.Success(tenant.Adapt<TenantResponse>(), "Tenant updated successfully.");
    }
}