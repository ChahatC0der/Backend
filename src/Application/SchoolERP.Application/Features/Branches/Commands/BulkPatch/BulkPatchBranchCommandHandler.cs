using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Shared.Results;
using SchoolERP.Domain.Tenants.Entities;
using System.Linq.Expressions;

namespace SchoolERP.Application.Features.Branches.Commands.BulkPatch;

public class BulkPatchBranchCommandHandler : IRequestHandler<BulkPatchBranchCommand, Result<int>>
{
    private readonly IApplicationDbContext _dbContext;

    public BulkPatchBranchCommandHandler(IApplicationDbContext dbContext)
        => _dbContext = dbContext;

    public async Task<Result<int>> Handle(BulkPatchBranchCommand request, CancellationToken cancellationToken)
    {
        var ids = request.Request.Ids.Distinct().ToList();

        // 1️⃣ FETCH BRANCHES
        var branches = await _dbContext.Set<Branch>()
            .Where(b => b.TenantId == request.TenantId && ids.Contains(b.Id) && !b.IsDeleted)
            .ToListAsync(cancellationToken);

        if (!branches.Any())
            return Error.NotFound("Branches", "No matching branches found.");

        // 2️⃣ UNIQUENESS CHECKS (EXCLUDE BRANCHES BEING UPDATED)
        var checks = new List<(Expression<Func<Branch, bool>> Predicate, string Message)>();

        if (!string.IsNullOrEmpty(request.Request.Code))
        {
            var code = request.Request.Code.Trim().ToUpper();
            checks.Add((b => b.Code == code && b.TenantId == request.TenantId && !ids.Contains(b.Id) && !b.IsDeleted,
                $"Branch code '{code}' already exists in this tenant."));
        }

        if (checks.Any())
        {
            var error = await _dbContext.EnsureAllUniqueAsync(checks, cancellationToken);
            if (error is not null)
                return error;
        }

        // 3️⃣ RESET OTHER DEFAULTS (IF SETTING DEFAULT)
        if (request.Request.IsDefault.HasValue && request.Request.IsDefault.Value)
        {
            var existingDefaults = await _dbContext.Set<Branch>()
                .Where(b => b.TenantId == request.TenantId && b.IsDefault && !ids.Contains(b.Id))
                .ToListAsync(cancellationToken);

            foreach (var d in existingDefaults)
                d.IsDefault = false;
        }

        // 4️⃣ 🔥 APPLY PATCH (SIRF PROVIDED FIELDS)
        foreach (var branch in branches)
        {
            request.Request.Name.PatchIfProvided(value => branch.Name = value);
            request.Request.Code.PatchIfProvided(value => branch.Code = value.Trim().ToUpper());
            request.Request.Address.PatchIfProvided(value => branch.Address = value);
            request.Request.Phone.PatchIfProvided(value => branch.Phone = value);
            request.Request.Email.PatchIfProvided(value => branch.Email = value);
            request.Request.ContactPerson.PatchIfProvided(value => branch.ContactPerson = value);
            if (request.Request.IsDefault.HasValue)
                branch.IsDefault = request.Request.IsDefault.Value;
            request.Request.Status.PatchIfProvided(value => branch.Status = value);
        }

        // 5️⃣ SAVE (AUTO AUDIT)
        await _dbContext.SaveChangesAsync(cancellationToken);

        // 6️⃣ RETURN
        return Result.Success(branches.Count, $"{branches.Count} branch(es) patched successfully.");
    }
}