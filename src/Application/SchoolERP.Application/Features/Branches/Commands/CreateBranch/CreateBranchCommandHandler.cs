using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Branches.DTOs;
using SchoolERP.Domain.Rbac.Entities;
using SchoolERP.Domain.Shared.Results;
using SchoolERP.Domain.Tenants.Entities;
using System.Linq.Expressions;

namespace SchoolERP.Application.Features.Branches.Commands.CreateBranch;

public class CreateBranchCommandHandler
    : IRequestHandler<CreateBranchCommand, Result<BranchResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentTenantService _currentTenantService;

    public CreateBranchCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentTenantService currentTenantService)
    {
        _dbContext = dbContext;
        _currentTenantService = currentTenantService;
    }

    public async Task<Result<BranchResponse>> Handle(
        CreateBranchCommand request,
        CancellationToken cancellationToken)
    {
        // 1️⃣ GET CURRENT TENANT FROM FINBUCKLE
        var tenantId = _currentTenantService.GetTenantId();

        if (tenantId == Guid.Empty)
            return Error.NotFound("Tenant",Convert.ToString(tenantId));

        // 2️⃣ CLEAN CODE
        var code = request.Request.Code?.Trim().ToUpper() ?? string.Empty;

        // 3️⃣ UNIQUENESS CHECK
        var checks =
            new (Expression<Func<Branch, bool>> Predicate, string Message)[]
            {
                (
                    b => b.Code == code &&
                         b.TenantId == tenantId &&
                         !b.IsDeleted,

                    $"Branch code '{code}' already exists in this tenant."
                )
            };

        var uniquenessError =
            await _dbContext.EnsureAllUniqueAsync(
                checks,
                cancellationToken);

        if (uniquenessError is not null)
            return uniquenessError;

        // 4️⃣ RESET OTHER DEFAULTS
        if (request.Request.IsDefault)
        {
            var existingDefaults = await _dbContext
                .Set<Branch>()
                .Where(b =>
                    b.TenantId == tenantId &&
                    b.IsDefault &&
                    !b.IsDeleted)
                .ToListAsync(cancellationToken);

            foreach (var item in existingDefaults)
                item.IsDefault = false;
        }

        // 5️⃣ CREATE
        var branch = request.Request.Adapt<Branch>();

        // TenantId is assigned centrally by AppDbContext.SaveChangesAsync()
        await _dbContext.Set<Branch>()
            .AddAsync(branch, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        // 6️⃣ RESPONSE
        var response = branch.Adapt<BranchResponse>();

        return Result.Success(
            response,
            "Branch created successfully.");
    }
}