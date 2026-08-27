using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Rbac.Entities;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Rbac.Commands.Assignment.UnassignRole;

public class UnassignRoleCommandHandler : IRequestHandler<UnassignRoleCommand, Result<bool>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentTenantService _tenantService;

    public UnassignRoleCommandHandler(IApplicationDbContext dbContext, ICurrentTenantService tenantService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
    }

    public async Task<Result<bool>> Handle(UnassignRoleCommand command, CancellationToken cancellationToken)
    {
        var tenantId = _tenantService.GetTenantId();
        var userRole = await _dbContext.Set<UserRole>()
            .FirstOrDefaultAsync(ur => ur.Id == command.UserRoleId && ur.TenantId == tenantId, cancellationToken);

        if (userRole == null)
            return Error.NotFound("UserRole", command.UserRoleId.ToString());

        userRole.ValidTo = DateTime.UtcNow.Date;
        // SaveChanges called by TransactionBehavior

        return Result.Success(true);
    }
}