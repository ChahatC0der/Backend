using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.AI.DTOs;

namespace SchoolERP.Infrastructure.Services.AI;

public sealed class HttpAiExecutionContextAccessor
    : IAiExecutionContextAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ICurrentTenantService _currentTenantService;
    private readonly IAiPermissionProvider _permissionProvider;
    private readonly IAiBranchContextProvider _branchContextProvider;

    public HttpAiExecutionContextAccessor(
        IHttpContextAccessor httpContextAccessor,
        ICurrentTenantService currentTenantService,
        IAiPermissionProvider permissionProvider,
        IAiBranchContextProvider branchContextProvider)
    {
        _httpContextAccessor = httpContextAccessor;
        _currentTenantService = currentTenantService;
        _permissionProvider = permissionProvider;
        _branchContextProvider = branchContextProvider;
    }

    public AiExecutionContext GetCurrent()
    {
        var httpContext = _httpContextAccessor.HttpContext;

        if (httpContext is null)
            return new AiExecutionContext();

        var user = httpContext.User;

        if (user.Identity?.IsAuthenticated != true)
        {
            return new AiExecutionContext
            {
                IsAuthenticated = false
            };
        }

        Guid? userId = null;

        var nameIdentifier =
            user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (Guid.TryParse(nameIdentifier, out var parsedUserId))
            userId = parsedUserId;

        var tenantId =
            _currentTenantService.GetTenantId();

        var branchId =
            _branchContextProvider.GetBranchId(user);

        var permissions =
            _permissionProvider.GetPermissions(user);

        return new AiExecutionContext
        {
            IsAuthenticated = true,
            UserId = userId,
            TenantId = tenantId,
            BranchId = branchId,
            Permissions = permissions
        };
    }
}