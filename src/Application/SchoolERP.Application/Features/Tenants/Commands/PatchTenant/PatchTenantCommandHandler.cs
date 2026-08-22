using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Tenants.DTOs;
using SchoolERP.Domain.Shared.Results;
using SchoolERP.Domain.Tenants.Entities;
using System.Linq.Expressions;

namespace SchoolERP.Application.Features.Tenants.Commands.PatchTenant;

public class PatchTenantCommandHandler : IRequestHandler<PatchTenantCommand, Result<TenantResponse>>
{
    private readonly IApplicationDbContext _dbContext;

    public PatchTenantCommandHandler(IApplicationDbContext dbContext)
        => _dbContext = dbContext;

    public async Task<Result<TenantResponse>> Handle(PatchTenantCommand request, CancellationToken cancellationToken)
    {
        // 1️⃣ FETCH TENANT (Helper)
        var tenantResult = await _dbContext.GetEntityByIdAsync<Tenant>(request.Id, cancellationToken);
        if (tenantResult.IsFailure)
            return tenantResult.Error;

        var tenant = tenantResult.Value;

        // 2️⃣ PREPARE UNIQUENESS CHECKS (SIRF TAB JAB FIELD UPDATE HO RAHI HO)
        var checks = new List<(Expression<Func<Tenant, bool>> Predicate, string Message)>();

        // Code uniqueness (if being updated)
        if (!string.IsNullOrEmpty(request.Request.Code))
        {
            var code = request.Request.Code.Trim().ToUpper();
            checks.Add((t => t.Code == code && t.Id != request.Id && !t.IsDeleted,
                $"Code '{code}' is already taken by another tenant."));
        }

        // Subdomain uniqueness (if being updated)
        if (!string.IsNullOrEmpty(request.Request.Subdomain))
        {
            var cleanSubdomain = request.Request.Subdomain.Trim().ToLower().Replace(" ", "-");
            checks.Add((t => t.Subdomain == cleanSubdomain && t.Id != request.Id && !t.IsDeleted,
                $"Subdomain '{cleanSubdomain}' is already taken by another tenant."));
        }

        // Email uniqueness (if being updated)
        if (!string.IsNullOrEmpty(request.Request.ContactEmail))
        {
            var email = request.Request.ContactEmail.Trim().ToLower();
            checks.Add((t => t.ContactEmail == email && t.Id != request.Id && !t.IsDeleted,
                $"Email '{email}' is already registered by another tenant."));
        }

        // Name uniqueness (optional, but good to have)
        if (!string.IsNullOrEmpty(request.Request.Name))
        {
            var name = request.Request.Name.Trim();
            checks.Add((t => t.Name == name && t.Id != request.Id && !t.IsDeleted,
                $"Name '{name}' is already taken by another tenant."));
        }

        // Run uniqueness checks
        if (checks.Any())
        {
            var error = await _dbContext.EnsureAllUniqueAsync(checks, cancellationToken);
            if (error is not null)
                return error;
        }

        request.Request.Code.PatchIfProvided(v => tenant.Code = v.ToUpper());
        request.Request.Name.PatchIfProvided(v => tenant.Name = v);
        request.Request.Subdomain.PatchIfProvided(v => tenant.Subdomain = v.ToLower().Replace(" ", "-"));
        request.Request.ContactEmail.PatchIfProvided(v => tenant.ContactEmail = v.ToLower());
        request.Request.ContactPhone.PatchIfProvided(v => tenant.ContactPhone = v);
        request.Request.Address.PatchIfProvided(v => tenant.Address = v);
        request.Request.Plan.PatchIfProvided(v => tenant.Plan = v.ToLower());
        request.Request.Status.PatchIfProvided(v => tenant.Status = v.ToLower());

        // 5️⃣ SAVE (UpdatedAt auto-set by SaveChangesAsync)
        await _dbContext.SaveChangesAsync(cancellationToken);

        // 6️⃣ MAP RESPONSE (Manual to avoid Mapster issues)
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

        return Result.Success(response, "Tenant updated successfully.");
    }
}