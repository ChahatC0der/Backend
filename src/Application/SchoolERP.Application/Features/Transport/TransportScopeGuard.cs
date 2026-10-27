using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Transport;

/// <summary>
/// Centralized scope guard for every Transport handler.
/// Mirrors Fees FeeScopeGuard behavior — ICurrentTenantService.GetTenantId() returns non-nullable Guid.
/// Handlers must start with one of these before touching DB.
/// </summary>
internal static class TransportScopeGuard
{
    public static Result<(Guid TenantId, Guid BranchId)> RequireBranch(
        ICurrentTenantService tenant,
        ICurrentBranchService branch)
    {
        var tenantId = tenant.GetTenantId();
        if (tenantId == Guid.Empty)
            return Error.Unauthorized("Tenant context is not resolved.");

        var branchId = branch.GetBranchId();
        if (branchId is null || branchId == Guid.Empty)
            return Error.Validation("Branch is not selected. Provide the BranchId header or JWT claim.");

        return (tenantId, branchId.Value);
    }

    public static Result<Guid> RequireTenant(ICurrentTenantService tenant)
    {
        var tenantId = tenant.GetTenantId();
        if (tenantId == Guid.Empty)
            return Error.Unauthorized("Tenant context is not resolved.");

        return tenantId;
    }

    public static Result<(Guid TenantId, Guid? BranchId)> GetBranchOptional(
        ICurrentTenantService tenant,
        ICurrentBranchService branch)
    {
        var tenantId = tenant.GetTenantId();
        if (tenantId == Guid.Empty)
            return Error.Unauthorized("Tenant context is not resolved.");

        var branchId = branch.GetBranchId();
        return (tenantId, branchId == Guid.Empty ? null : branchId);
    }
}
