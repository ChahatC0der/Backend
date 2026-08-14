using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Tenants.DTOs;
using SchoolERP.Domain.Shared.Results;
using SchoolERP.Domain.Tenants.Entities;

namespace SchoolERP.Application.Features.Tenants.Commands.CreateTenant;

public class CreateTenantCommandHandler : IRequestHandler<CreateTenantCommand, Result<TenantResponse>>
{
    private readonly IApplicationDbContext _dbContext;

    public CreateTenantCommandHandler(IApplicationDbContext dbContext)
        => _dbContext = dbContext;

    public async Task<Result<TenantResponse>> Handle(CreateTenantCommand request, CancellationToken cancellationToken)
    {
        // 1️⃣ Generate subdomain
        var subdomain = request.Request.Name.ToLower().Replace(" ", "-");

        // 2️⃣ Check uniqueness (soft-deleted tenants should also be blocked for reuse)
        var exists = await _dbContext.Set<Tenant>()
            .AnyAsync(t => t.Subdomain == subdomain && !t.IsDeleted, cancellationToken);

        if (exists)
            return (Result<TenantResponse>)Result<TenantResponse>.Failure(
                Error.Conflict($"Subdomain '{subdomain}' is already taken."));

        // 1️⃣ Generate subdomain
        var email = request.Request.ContactEmail;

        // 2️⃣ Check uniqueness (soft-deleted tenants should also be blocked for reuse)
        var emailexists = await _dbContext.Set<Tenant>()
            .AnyAsync(t => t.ContactEmail == email && !t.IsDeleted, cancellationToken);

        if (emailexists)
            return (Result<TenantResponse>)Result<TenantResponse>.Failure(
                Error.Conflict($"Email '{email}' is already taken."));

        // 3️⃣ Create tenant entity
        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = request.Request.Name,
            Subdomain = subdomain,
            ContactEmail = request.Request.ContactEmail,
            ContactPhone = request.Request.ContactPhone,
            Address = request.Request.Address,
            Plan = request.Request.Plan,
            Status = "active",
            Settings = "{}",
            CustomFieldsDef = "{}",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // 4️⃣ Save
        await _dbContext.Set<Tenant>().AddAsync(tenant, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // 5️⃣ Return response
        return Result.Success(tenant.Adapt<TenantResponse>(), "Tenant created successfully.");
    }
}