using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Rbac.DTOs;

public record BulkAssignRoleCommand(
    Guid TenantId,
    long RoleId,
    string ScopeType,
    string? ScopeValue,
    List<long> UserIds
) : ICommand<BulkRoleJobResponse>;