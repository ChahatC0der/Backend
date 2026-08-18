using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Branches.DTOs;
using SchoolERP.Domain.Shared.Results;
using SchoolERP.Domain.Tenants.Entities;

namespace SchoolERP.Application.Features.Branches.Commands.CreateBranch;

public class CreateBranchCommandHandler : IRequestHandler<CreateBranchCommand, Result<BranchResponse>>
{
    private readonly IApplicationDbContext _dbContext;

    public CreateBranchCommandHandler(IApplicationDbContext dbContext) => _dbContext = dbContext;

    public async Task<Result<BranchResponse>> Handle(CreateBranchCommand request, CancellationToken cancellationToken)
    {
        // 1. Check Tenant exists
        var tenantExists = await _dbContext.Set<Tenant>()
            .AnyAsync(t => t.Id == request.TenantId && !t.IsDeleted, cancellationToken);

        if (!tenantExists)
            return Error.NotFound("Tenant", request.TenantId.ToString());

        // 2. Check Code uniqueness within tenant
        var codeExists = await _dbContext.Set<Branch>()
            .AnyAsync(b => b.TenantId == request.TenantId && b.Code == request.Request.Code && !b.IsDeleted, cancellationToken);

        if (codeExists)
            return Error.Conflict($"Branch code '{request.Request.Code}' already exists in this tenant.");

        // 3. If setting as Default, reset other defaults for this tenant
        if (request.Request.IsDefault)
        {
            var existingDefaults = await _dbContext.Set<Branch>()
                .Where(b => b.TenantId == request.TenantId && b.IsDefault)
                .ToListAsync(cancellationToken);
            foreach (var item in existingDefaults)
                item.IsDefault = false;
        }

        // 4. Create Branch
        var branch = request.Request.Adapt<Branch>();
        branch.Id = Guid.NewGuid();
        branch.TenantId = request.TenantId;
        branch.Status = "active";
        branch.Settings = "{}";

        await _dbContext.Set<Branch>().AddAsync(branch, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(branch.Adapt<BranchResponse>(), "Branch created successfully.");
    }
}