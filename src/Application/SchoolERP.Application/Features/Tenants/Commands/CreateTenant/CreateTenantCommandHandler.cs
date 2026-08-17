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
        var subdomain = request.Request.Subdomain.ToLower().Replace(" ", "-");

        // 🔥 1. Code Uniqueness Check
        var codeExists = await _dbContext.Set<Tenant>()
            .AnyAsync(t => t.Code == request.Request.Code && !t.IsDeleted, cancellationToken);
        if (codeExists)
            return Error.Conflict($"Code '{request.Request.Code}' is already taken.");

        var nameExists = await _dbContext.Set<Tenant>()
            .AnyAsync(t => t.Name == request.Request.Name && !t.IsDeleted, cancellationToken);
        if (nameExists)
            return Error.Conflict($"Code '{request.Request.Name}' is already taken.");

        // 🔥 2. Subdomain Uniqueness Check
        var subdomainExists = await _dbContext.Set<Tenant>()
            .AnyAsync(t => t.Subdomain.ToLower() == subdomain && !t.IsDeleted, cancellationToken);
        if (subdomainExists)
            return Error.Conflict($"Subdomain '{subdomain}' is already taken.");

        // 🔥 3. Email Uniqueness Check
        var emailExists = await _dbContext.Set<Tenant>()
            .AnyAsync(t => t.ContactEmail.ToLower() == request.Request.ContactEmail.ToLower() && !t.IsDeleted, cancellationToken);
        if (emailExists)
            return Error.Conflict($"Email '{request.Request.ContactEmail}' is already registered.");

        // 🔥 4. Create Tenant
        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Code = request.Request.Code,    // 👈 USER KA CODE
            Name = request.Request.Name,
            Subdomain = subdomain,
            ContactEmail = request.Request.ContactEmail,
            ContactPhone = request.Request.ContactPhone,
            Address = request.Request.Address,
            Plan = request.Request.Plan,
            Status = "active",
            Settings = "{}",
            CustomFieldsDef = "{}"
        };

        await _dbContext.Set<Tenant>().AddAsync(tenant, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(tenant.Adapt<TenantResponse>(), "Tenant created successfully.");
    }
}