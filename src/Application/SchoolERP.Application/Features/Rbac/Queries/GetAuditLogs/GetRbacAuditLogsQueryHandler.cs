using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Common.DTOs;
using SchoolERP.Application.Features.Rbac.DTOs;
using SchoolERP.Domain.Rbac.Entities;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Rbac.Queries.AuditLog.GetRbacAuditLogs;

public class GetRbacAuditLogsQueryHandler : IRequestHandler<GetRbacAuditLogsQuery, Result<PagedResponse<RbacAuditLogResponse>>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentTenantService _tenantService;

    public GetRbacAuditLogsQueryHandler(IApplicationDbContext dbContext, ICurrentTenantService tenantService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
    }

    public async Task<Result<PagedResponse<RbacAuditLogResponse>>> Handle(GetRbacAuditLogsQuery query, CancellationToken cancellationToken)
    {
        var request = query.Request;
        var tenantId = request.TenantId ?? _tenantService.GetTenantId();

        var logsQuery = _dbContext.Set<RbacAuditLog>()
            .AsNoTracking()
            .Where(l => l.TenantId == tenantId);

        if (request.UserId.HasValue)
            logsQuery = logsQuery.Where(l => l.PerformedBy == request.UserId.Value || l.AffectedUserId == request.UserId.Value);

        if (request.RoleId.HasValue)
            logsQuery = logsQuery.Where(l => l.AffectedRoleId == request.RoleId.Value);

        if (!string.IsNullOrWhiteSpace(request.Action))
            logsQuery = logsQuery.Where(l => l.Action == request.Action);

        if (request.FromDate.HasValue)
            logsQuery = logsQuery.Where(l => l.CreatedAt >= request.FromDate.Value);

        if (request.ToDate.HasValue)
            logsQuery = logsQuery.Where(l => l.CreatedAt <= request.ToDate.Value);

        logsQuery = request.SortBy?.ToLower() switch
        {
            "createdat" => request.SortOrder?.ToLower() == "desc" ? logsQuery.OrderByDescending(l => l.CreatedAt) : logsQuery.OrderBy(l => l.CreatedAt),
            _ => logsQuery.OrderByDescending(l => l.CreatedAt)
        };

        var totalCount = await logsQuery.CountAsync(cancellationToken);
        var items = await logsQuery
            .Skip((request.Page - 1) * request.Size)
            .Take(request.Size)
            .ToListAsync(cancellationToken);

        var data = items.Select(l => l.Adapt<RbacAuditLogResponse>()).ToList();

        return Result.Success(new PagedResponse<RbacAuditLogResponse>
        {
            Data = data,
            TotalCount = totalCount,
            Page = request.Page,
            Size = request.Size
        });
    }
}