using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Shared.Results;
using SchoolERP.Domain.Tenants.Entities;

namespace SchoolERP.Application.Features.Tenants.Commands.DeleteTenant;

public class DeleteTenantCommandHandler : IRequestHandler<DeleteTenantCommand, Result<bool>>
{
    private readonly IApplicationDbContext _dbContext;

    public DeleteTenantCommandHandler(IApplicationDbContext dbContext)
        => _dbContext = dbContext;

    public async Task<Result<bool>> Handle(DeleteTenantCommand request, CancellationToken cancellationToken)
    {
        // 1️⃣ FETCH TENANT (Helper)
        var tenantResult = await _dbContext.GetEntityByIdAsync<Tenant>(request.Id, cancellationToken);
        if (tenantResult.IsFailure)
            return tenantResult.Error;

        var tenant = tenantResult.Value;

        // 2️⃣ SOFT DELETE
        tenant.IsDeleted = true;
        tenant.DeletedAt = DateTime.UtcNow;

        // 🔥 UpdatedAt auto-set by SaveChangesAsync override (no need to manually set)

        // 3️⃣ SAVE
        await _dbContext.SaveChangesAsync(cancellationToken);

        // 4️⃣ RETURN
        return Result.Success(true, $"Tenant '{tenant.Name}' deleted successfully.");
    }
}