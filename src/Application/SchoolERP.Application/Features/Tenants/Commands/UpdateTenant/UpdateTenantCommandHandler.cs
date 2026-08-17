using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Tenants.DTOs;
using SchoolERP.Domain.Shared.Results;
using SchoolERP.Domain.Tenants.Entities;

namespace SchoolERP.Application.Features.Tenants.Commands.UpdateTenant;

public class UpdateTenantCommandHandler : IRequestHandler<UpdateTenantCommand, Result<TenantResponse>>
{
    private readonly IApplicationDbContext _dbContext;

    public UpdateTenantCommandHandler(IApplicationDbContext dbContext)
        => _dbContext = dbContext;

    public async Task<Result<TenantResponse>> Handle(UpdateTenantCommand request, CancellationToken cancellationToken)
    {
        // 1️⃣ Fetch existing tenant
        var tenant = await _dbContext.Set<Tenant>()
            .FirstOrDefaultAsync(t => t.Id == request.Id && !t.IsDeleted, cancellationToken);

        if (tenant == null)
            return Error.NotFound("Tenant", request.Id.ToString());

        // 🔥 2️⃣ Uniqueness Checks (EXCLUDE current tenant)
        var codeExists = await _dbContext.Set<Tenant>()
            .AnyAsync(t => t.Code == request.Request.Code && t.Id != request.Id && !t.IsDeleted, cancellationToken);
        if (codeExists)
            return Error.Conflict($"Code '{request.Request.Code}' is already taken by another tenant.");

        var nameExists = await _dbContext.Set<Tenant>()
            .AnyAsync(t => t.Name == request.Request.Name && t.Id != request.Id && !t.IsDeleted, cancellationToken);
        if (nameExists)
            return Error.Conflict($"Name '{request.Request.Name}' is already taken by another tenant.");

        var cleanSubdomain = request.Request.Subdomain.Trim().ToLower().Replace(" ", "-");
        var subdomainExists = await _dbContext.Set<Tenant>()
            .AnyAsync(t => t.Subdomain == cleanSubdomain && t.Id != request.Id && !t.IsDeleted, cancellationToken);
        if (subdomainExists)
            return Error.Conflict($"Subdomain '{cleanSubdomain}' is already taken by another tenant.");

        var emailExists = await _dbContext.Set<Tenant>()
            .AnyAsync(t => t.ContactEmail.ToLower() == request.Request.ContactEmail.ToLower() && t.Id != request.Id && !t.IsDeleted, cancellationToken);
        if (emailExists)
            return Error.Conflict($"Email '{request.Request.ContactEmail}' is already registered by another tenant.");

        // 3️⃣ Update fields
        tenant.Code = request.Request.Code;
        tenant.Name = request.Request.Name;
        tenant.Subdomain = cleanSubdomain;
        tenant.ContactEmail = request.Request.ContactEmail;
        tenant.ContactPhone = request.Request.ContactPhone;
        tenant.Address = request.Request.Address;
        tenant.Plan = request.Request.Plan;
        tenant.Status = request.Request.Status;
        tenant.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(tenant.Adapt<TenantResponse>(), "Tenant updated successfully.");
    }
}