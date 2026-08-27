using Mapster;
using MediatR;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Rbac.DTOs;
using SchoolERP.Domain.Rbac.Entities;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Rbac.Commands.Assignment.BulkAssignRole;

public class BulkAssignRoleCommandHandler : IRequestHandler<BulkAssignRoleCommand, Result<BulkRoleJobResponse>>
{
    private readonly IApplicationDbContext _dbContext;

    public BulkAssignRoleCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<BulkRoleJobResponse>> Handle(BulkAssignRoleCommand command, CancellationToken cancellationToken)
    {
        // Create a bulk job record (processing would be done via background job)
        var job = new BulkRoleJob
        {
            TenantId = command.TenantId,
            CreatedBy = 0, // TODO: Use ICurrentUserService to get user ID
            RoleId = command.RoleId,
            ScopeType = command.ScopeType,
            ScopeValue = command.ScopeValue ?? string.Empty,
            TotalUsers = command.UserIds.Count,
            Status = "pending"
        };

        _dbContext.Set<BulkRoleJob>().Add(job);
        // SaveChanges called by TransactionBehavior

        return Result.Success(job.Adapt<BulkRoleJobResponse>());
    }
}