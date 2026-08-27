using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Branches.DTOs;
using SchoolERP.Domain.Shared.Results;
using SchoolERP.Domain.Tenants.Entities;
using System.Linq.Expressions;

namespace SchoolERP.Application.Features.Branches.Commands.CreateBranch;

public class CreateBranchCommandHandler : IRequestHandler<CreateBranchCommand, Result<BranchResponse>>
{
    private readonly IApplicationDbContext _dbContext;

    public CreateBranchCommandHandler(IApplicationDbContext dbContext) => _dbContext = dbContext;

    public async Task<Result<BranchResponse>> Handle(CreateBranchCommand request, CancellationToken cancellationToken)
    {
        // 1️⃣ CLEAN CODE
        var code = request.Request.Code?.Trim().ToUpper() ?? string.Empty;

        // 2️⃣ CHECK TENANT EXISTS (Using Helper)
        var tenantExistsError = await _dbContext.EnsureEntityExistsAsync<Tenant>(request.TenantId, cancellationToken);
        if (tenantExistsError is not null)
            return tenantExistsError;

        // 3️⃣ UNIQUENESS CHECK (Using Helper)
        var checks = new (Expression<Func<Branch, bool>> Predicate, string Message)[]
        {
            (b => b.Code == code && b.TenantId == request.TenantId && !b.IsDeleted,
                $"Branch code '{code}' already exists in this tenant.")
        };

        var uniquenessError = await _dbContext.EnsureAllUniqueAsync(checks, cancellationToken);
        if (uniquenessError is not null)
            return uniquenessError;

        // 4️⃣ RESET OTHER DEFAULTS (if setting default)
        if (request.Request.IsDefault)
        {
            var existingDefaults = await _dbContext.Set<Branch>()
                .Where(b => b.TenantId == request.TenantId && b.IsDefault)
                .ToListAsync(cancellationToken);
            foreach (var item in existingDefaults)
                item.IsDefault = false;
        }

        // 5️⃣ CREATE
        var branch = request.Request.Adapt<Branch>();
       // branch.TenantId = request.TenantId;

        await _dbContext.Set<Branch>().AddAsync(branch, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // 6️⃣ RESPONSE
        var response = branch.Adapt<BranchResponse>();
        return Result.Success(response, "Branch created successfully.");
    }
}