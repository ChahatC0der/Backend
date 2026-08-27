using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Shared.Results;
using SchoolERP.Domain.Tenants.Entities;
using System.Linq.Expressions;

namespace SchoolERP.Application.Features.Tenants.Commands.BulkPatch;

public class BulkPatchTenantCommandHandler : IRequestHandler<BulkPatchTenantCommand, Result<int>>
{
    private readonly IApplicationDbContext _dbContext;

    public BulkPatchTenantCommandHandler(IApplicationDbContext dbContext)
        => _dbContext = dbContext;

    public async Task<Result<int>> Handle(BulkPatchTenantCommand request, CancellationToken cancellationToken)
    {
        var ids = request.Request.Ids.Distinct().ToList();

        // 1️⃣ FETCH TENANTS
        var tenants = await _dbContext.Set<Tenant>()
            .Where(t => ids.Contains(t.Id) && !t.IsDeleted)
            .ToListAsync(cancellationToken);

        if (!tenants.Any())
            return Error.NotFound("Tenants", "No matching tenants found.");

        // 2️⃣ UNIQUENESS CHECKS (EXCLUDE TENANTS BEING UPDATED)
        var checks = new List<(Expression<Func<Tenant, bool>> Predicate, string Message)>();

        if (!string.IsNullOrEmpty(request.Request.Code))
        {
            var code = request.Request.Code.Trim().ToUpper();
            checks.Add((t => t.Code == code && !ids.Contains(t.Id) && !t.IsDeleted,
                $"Code '{code}' is already taken by another tenant."));
        }

        if (!string.IsNullOrEmpty(request.Request.Name))
        {
            var name = request.Request.Name.Trim();
            checks.Add((t => t.Name == name && !ids.Contains(t.Id) && !t.IsDeleted,
                $"Name '{name}' is already taken by another tenant."));
        }

        if (!string.IsNullOrEmpty(request.Request.Subdomain))
        {
            var cleanSubdomain = request.Request.Subdomain.Trim().ToLower().Replace(" ", "-");
            checks.Add((t => t.Subdomain == cleanSubdomain && !ids.Contains(t.Id) && !t.IsDeleted,
                $"Subdomain '{cleanSubdomain}' is already taken by another tenant."));
        }

        if (!string.IsNullOrEmpty(request.Request.ContactEmail))
        {
            var email = request.Request.ContactEmail.Trim().ToLower();
            checks.Add((t => t.ContactEmail == email && !ids.Contains(t.Id) && !t.IsDeleted,
                $"Email '{email}' is already registered by another tenant."));
        }

        if (checks.Any())
        {
            var error = await _dbContext.EnsureAllUniqueAsync(checks, cancellationToken);
            if (error is not null)
                return error;
        }

        // 3️⃣ 🔥 APPLY PATCH (FLUENT HELPER — 1 LINER PER FIELD)
        foreach (var tenant in tenants)
        {
            request.Request.Code.PatchIfProvided(value => tenant.Code = value);
            request.Request.Name.PatchIfProvided(value => tenant.Name = value);
            request.Request.Subdomain.PatchIfProvided(value => tenant.Subdomain = value.Trim().ToLower().Replace(" ", "-"));
            request.Request.ContactEmail.PatchIfProvided(value => tenant.ContactEmail = value);
            request.Request.ContactPhone.PatchIfProvided(value => tenant.ContactPhone = value);
            request.Request.Address.PatchIfProvided(value => tenant.Address = value);
            request.Request.Plan.PatchIfProvided(value => tenant.Plan = value);
            request.Request.Status.PatchIfProvided(value => tenant.Status = value);
        }

        // 4️⃣ SAVE (AUTO AUDIT - UpdatedAt set by SaveChangesAsync override)
        await _dbContext.SaveChangesAsync(cancellationToken);

        // 5️⃣ RETURN
        return Result.Success(tenants.Count, $"{tenants.Count} tenant(s) patched successfully.");
    }
}