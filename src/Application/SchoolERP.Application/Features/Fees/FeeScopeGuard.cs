using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Fees;

/// <summary>
/// Centralized scope guard for every Fees handler.
///
/// ICurrentTenantService.GetTenantId() returns Guid (non-nullable).
/// Empty Guid = no tenant resolved.
/// ICurrentBranchService.GetBranchId() returns Guid? — branch is optional.
/// </summary>
internal static class FeeScopeGuard
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
