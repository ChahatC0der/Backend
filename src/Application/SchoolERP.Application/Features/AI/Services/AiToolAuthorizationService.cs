using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.AI.Tools;

namespace SchoolERP.Application.Features.AI.Services;

public sealed class AiToolAuthorizationService
{
    private readonly IAiExecutionContextAccessor _contextAccessor;
    private readonly AiToolScopeAuthorizationService _scopeAuthorizationService;

    public AiToolAuthorizationService(
        IAiExecutionContextAccessor contextAccessor,
        AiToolScopeAuthorizationService scopeAuthorizationService)
    {
        _contextAccessor = contextAccessor;
        _scopeAuthorizationService = scopeAuthorizationService;
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

        if (!context.IsAuthenticated)
        {
            return AiToolAuthorizationResult.Denied(
                "Authentication is required to use AI tools.");
        }

        if (!string.IsNullOrWhiteSpace(tool.RequiredPermission))
        {
            if (!context.Permissions.Contains(
                    tool.RequiredPermission))
            {
                return AiToolAuthorizationResult.Denied(
                    $"Permission '{tool.RequiredPermission}' is required.");
            }
        }

        var scopeResult =
            _scopeAuthorizationService.Authorize(tool);

        if (!scopeResult.IsAllowed)
        {
            return scopeResult;
        }

        return AiToolAuthorizationResult.Allowed();
    }
}