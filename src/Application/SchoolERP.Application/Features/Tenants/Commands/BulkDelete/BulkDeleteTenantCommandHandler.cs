using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Shared.Results;
using SchoolERP.Domain.Tenants.Entities;

namespace SchoolERP.Application.Features.Tenants.Commands.BulkDelete;

public class BulkDeleteTenantCommandHandler : IRequestHandler<BulkDeleteTenantCommand, Result<int>>
{
    private readonly IApplicationDbContext _dbContext;

    public BulkDeleteTenantCommandHandler(IApplicationDbContext dbContext)
        => _dbContext = dbContext;

    public async Task<Result<int>> Handle(BulkDeleteTenantCommand request, CancellationToken cancellationToken)
    {
        // 1️⃣ FETCH TENANTS
        var tenants = await _dbContext.Set<Tenant>()
            .Where(t => request.Request.Ids.Contains(t.Id) && !t.IsDeleted)
            .ToListAsync(cancellationToken);

        if (!tenants.Any())
            return Error.NotFound("Tenants", "No matching tenants found.");

        // 2️⃣ SOFT DELETE
        var count = tenants.Count;
        foreach (var tenant in tenants)
        {
            tenant.IsDeleted = true;
            tenant.DeletedAt = DateTime.UtcNow;
            // 🔥 UpdatedAt auto-set by SaveChangesAsync override (no manual set needed)
        }

        // 3️⃣ SAVE (AUTO AUDIT)
        await _dbContext.SaveChangesAsync(cancellationToken);

        // 4️⃣ RETURN
        return Result.Success(count, $"{count} tenant(s) deleted successfully.");
    }
}