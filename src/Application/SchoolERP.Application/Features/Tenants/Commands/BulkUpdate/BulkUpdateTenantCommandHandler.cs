using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Shared.Results;
using SchoolERP.Domain.Tenants.Entities;

namespace SchoolERP.Application.Features.Tenants.Commands.BulkUpdate;

public class BulkUpdateTenantCommandHandler : IRequestHandler<BulkUpdateTenantCommand, Result<int>>
{
    private readonly IApplicationDbContext _dbContext;

    public BulkUpdateTenantCommandHandler(IApplicationDbContext dbContext)
        => _dbContext = dbContext;

    public async Task<Result<int>> Handle(BulkUpdateTenantCommand request, CancellationToken cancellationToken)
    {
        var ids = request.Request.Ids.Distinct().ToList();

        // 1️⃣ Fetch tenants
        var tenants = await _dbContext.Set<Tenant>()
            .Where(t => ids.Contains(t.Id) && !t.IsDeleted)
            .ToListAsync(cancellationToken);

        if (!tenants.Any())
            return  Error.NotFound("Tenants", "No matching tenants found.");    

        // 2️⃣ 🔥 UNIQUENESS CHECKS (Exclude the tenants being updated)
        if (!string.IsNullOrEmpty(request.Request.Code))
        {
            var codeExists = await _dbContext.Set<Tenant>()
                .AnyAsync(t => t.Code == request.Request.Code && !ids.Contains(t.Id) && !t.IsDeleted, cancellationToken);
            if (codeExists)
                return  Error.Conflict($"Code '{request.Request.Code}' is already taken by another tenant.");
        }

        if (!string.IsNullOrEmpty(request.Request.Name))
        {
            var nameExists = await _dbContext.Set<Tenant>()
                .AnyAsync(t => t.Name == request.Request.Name && !ids.Contains(t.Id) && !t.IsDeleted, cancellationToken);
            if (nameExists)
                return  Error.Conflict($"Name '{request.Request.Name}' is already taken by another tenant.");
        }

        if (!string.IsNullOrEmpty(request.Request.Subdomain))
        {
            var cleanSubdomain = request.Request.Subdomain.Trim().ToLower().Replace(" ", "-");
            var subdomainExists = await _dbContext.Set<Tenant>()
                .AnyAsync(t => t.Subdomain == cleanSubdomain && !ids.Contains(t.Id) && !t.IsDeleted, cancellationToken);
            if (subdomainExists)
                return Error.Conflict($"Subdomain '{cleanSubdomain}' is already taken by another tenant.");
        }

        if (!string.IsNullOrEmpty(request.Request.ContactEmail))
        {
            var emailExists = await _dbContext.Set<Tenant>()
                .AnyAsync(t => t.ContactEmail.ToLower() == request.Request.ContactEmail.ToLower() && !ids.Contains(t.Id) && !t.IsDeleted, cancellationToken);
            if (emailExists)
                return  Error.Conflict($"Email '{request.Request.ContactEmail}' is already registered by another tenant.");
        }

        // 3️⃣ Apply updates
        foreach (var tenant in tenants)
        {
            if (!string.IsNullOrEmpty(request.Request.Code))
                tenant.Code = request.Request.Code;

            if (!string.IsNullOrEmpty(request.Request.Name))
                tenant.Name = request.Request.Name;

            if (!string.IsNullOrEmpty(request.Request.Subdomain))
                tenant.Subdomain = request.Request.Subdomain.Trim().ToLower().Replace(" ", "-");

            if (!string.IsNullOrEmpty(request.Request.ContactEmail))
                tenant.ContactEmail = request.Request.ContactEmail;

            if (!string.IsNullOrEmpty(request.Request.ContactPhone))
                tenant.ContactPhone = request.Request.ContactPhone;

            if (!string.IsNullOrEmpty(request.Request.Address))
                tenant.Address = request.Request.Address;

            if (!string.IsNullOrEmpty(request.Request.Plan))
                tenant.Plan = request.Request.Plan;

            if (!string.IsNullOrEmpty(request.Request.Status))
                tenant.Status = request.Request.Status;

            tenant.UpdatedAt = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(tenants.Count, $"{tenants.Count} tenant(s) updated successfully.");
    }
}