using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Shared.Results;
using SchoolERP.Domain.Tenants.Entities;

namespace SchoolERP.Application.Features.Tenants.Commands.RestoreTenant;

public class RestoreTenantCommandHandler : IRequestHandler<RestoreTenantCommand, Result<bool>>
{
    private readonly IApplicationDbContext _dbContext;

    public RestoreTenantCommandHandler(IApplicationDbContext dbContext)
        => _dbContext = dbContext;

    public async Task<Result<bool>> Handle(RestoreTenantCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _dbContext.Set<Tenant>()
            .FirstOrDefaultAsync(t => t.Id == request.Id && t.IsDeleted, cancellationToken);

        if (tenant == null)
            return Error.NotFound("Tenant", request.Id.ToString());

        tenant.IsDeleted = false;
        tenant.DeletedAt = null;
        tenant.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(true, $"Tenant '{tenant.Name}' restored successfully.");
    }
}