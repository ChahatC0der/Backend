using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.AI.DTOs;
using SchoolERP.Application.Features.AI.Tools;

namespace SchoolERP.Application.Features.AI.Services;

public sealed class AiToolScopeAuthorizationService
{
    private readonly IAiExecutionContextAccessor _contextAccessor;

    public AiToolScopeAuthorizationService(
        IAiExecutionContextAccessor contextAccessor)
    {
        _contextAccessor = contextAccessor;
    }

    public AiToolAuthorizationResult Authorize(
        ToolDefinition tool)
    {
        if (tool is null)
        {
            return AiToolAuthorizationResult.Denied(
                "Tool definition is required.");
        }

        var context = _contextAccessor.GetCurrent();

        return tool.DataScope switch
        {
            AiDataScope.None =>
                AiToolAuthorizationResult.Allowed(),

            AiDataScope.User =>
                AuthorizeUserScope(context),

            AiDataScope.Branch =>
                AuthorizeBranchScope(context),

            AiDataScope.Tenant =>
                AuthorizeTenantScope(context),

            _ =>
                AiToolAuthorizationResult.Denied(
                    $"Unsupported AI data scope '{tool.DataScope}'.")
        };
    }

    private static AiToolAuthorizationResult AuthorizeUserScope(
        AiExecutionContext context)
    {
        if (!context.UserId.HasValue)
        {
            return AiToolAuthorizationResult.Denied(
                "Current user context is required for this tool.");
        }

        return AiToolAuthorizationResult.Allowed();
    }

    private static AiToolAuthorizationResult AuthorizeBranchScope(
        AiExecutionContext context)
    {
        if (!context.TenantId.HasValue)
        {
            return AiToolAuthorizationResult.Denied(
                "Current tenant context is required for branch-scoped tools.");
        }

        if (!context.BranchId.HasValue)
        {
            return AiToolAuthorizationResult.Denied(
                "Current branch context is required for branch-scoped tools.");
        }

        return AiToolAuthorizationResult.Allowed();
    }

    private static AiToolAuthorizationResult AuthorizeTenantScope(
        AiExecutionContext context)
    {
        if (!context.TenantId.HasValue)
        {
            return AiToolAuthorizationResult.Denied(
                "Current tenant context is required for tenant-scoped tools.");
        }

        return AiToolAuthorizationResult.Allowed();
    }
}