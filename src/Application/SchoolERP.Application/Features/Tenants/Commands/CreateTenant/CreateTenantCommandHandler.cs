using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Tenants.DTOs;
using SchoolERP.Domain.Shared.Results;
using SchoolERP.Domain.Tenants.Entities;
using System.Linq.Expressions;

namespace SchoolERP.Application.Features.Tenants.Commands.CreateTenant;

public class CreateTenantCommandHandler : IRequestHandler<CreateTenantCommand, Result<TenantResponse>>
{
    private readonly IApplicationDbContext _dbContext;

    public CreateTenantCommandHandler(IApplicationDbContext dbContext)
        => _dbContext = dbContext;

    public async Task<Result<TenantResponse>> Handle(CreateTenantCommand request, CancellationToken cancellationToken)
    {
        var subdomain = request.Request.Subdomain.ToLower().Replace(" ", "-");
        var code = request.Request.Code.Trim();
        var name = request.Request.Name.Trim();
        var email = request.Request.ContactEmail.Trim().ToLower();
        
        var plan = request.Request.Plan.Trim().ToLowerInvariant();

        // ==========================================================
        // 🔥 STEP 1: Array Banao — Har Element Ek (Condition, Message) Pair Hai
        // ==========================================================
        var checks = new (Expression<Func<Tenant, bool>> Predicate, string Message)[]
        {
            (t => t.Code.ToLower() == code.ToLower() && !t.IsDeleted,
                $"A tenant with the code '{code}' already exists."),
            (t => t.Name.ToLower() == name.ToLower() && !t.IsDeleted,
                $"A tenant with the name '{name}' already exists."),

            (t => t.Subdomain == subdomain && !t.IsDeleted,
                $"Subdomain '{subdomain}' is already taken."),

            (t => t.ContactEmail.ToLower() == email && !t.IsDeleted,
                $"A tenant with email '{email}' already exists."),
        };

        // ==========================================================
        // 🔥 STEP 2: Ek Hi Call Mein Saare Checks Chala Do
        // ==========================================================
        var uniquenessError = await _dbContext.EnsureAllUniqueAsync(checks, cancellationToken);

        // ==========================================================
        // 🔥 STEP 3: Agar Koi Bhi Duplicate Mila, Turant Return Kar Do
        // ==========================================================
        if (uniquenessError is not null)
            return uniquenessError;


        //// 🔥 Ab har check ek hi line mein
        //if (await _dbContext.EnsureUniqueAsync<Tenant>(
        //        t => t.Code == request.Request.Code,
        //        $"Code '{request.Request.Code}' is already taken.", cancellationToken)
        //    is { } codeError)
        //    return codeError;

        //if (await _dbContext.EnsureUniqueAsync<Tenant>(
        //        t => t.Name == request.Request.Name,
        //        $"Name '{request.Request.Name}' is already taken.", cancellationToken)
        //    is { } nameError)
        //    return nameError;

        //if (await _dbContext.EnsureUniqueAsync<Tenant>(
        //        t => t.Subdomain.ToLower() == subdomain,
        //        $"Subdomain '{subdomain}' is already taken.", cancellationToken)
        //    is { } subdomainError)
        //    return subdomainError;

        //if (await _dbContext.EnsureUniqueAsync<Tenant>(
        //        t => t.ContactEmail.ToLower() == request.Request.ContactEmail.ToLower(),
        //        $"Email '{request.Request.ContactEmail}' is already registered.", cancellationToken)
        //    is { } emailError)
        //    return emailError;

        // 🔥 4. Create Tenant (Auto-Map via Mapster)
        var tenant = request.Request.Adapt<Tenant>();

        //// 🔥 4. Create Tenant
        //var tenant = new Tenant
        //{
        //    Id = Guid.NewGuid(),
        //    Code = request.Request.Code,    // 👈 USER KA CODE
        //    Name = request.Request.Name,
        //    Subdomain = request.Request.Subdomain,
        //    ContactEmail = request.Request.ContactEmail,
        //    ContactPhone = request.Request.ContactPhone,
        //    Address = request.Request.Address,
        //    Plan = request.Request.Plan,
        //    Status = "active",
        //    Settings = "{}",
        //    CustomFieldsDef = "{}"
        //};

        await _dbContext.Set<Tenant>().AddAsync(tenant, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(tenant.Adapt<TenantResponse>(), "Tenant created successfully.");
    }
}