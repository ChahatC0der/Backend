using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.Application.Features.Rbac.DTOs;

namespace SchoolERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AssignmentsController : BaseApiController
{
    private readonly IMediator _mediator;

    public AssignmentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // User-Role Assignment
    [HttpPost("user-role")]
    [AllowAnonymous] // TODO: [HasPermission("role.assign")]
    public async Task<IActionResult> AssignRoleToUser([FromBody] AssignRoleRequest request)
        => HandleResult(await _mediator.Send(new AssignRoleCommand(request)));

    [HttpDelete("user-role/{userRoleId:long}")]
    [AllowAnonymous] // TODO: [HasPermission("role.unassign")]
    public async Task<IActionResult> UnassignRoleFromUser(long userRoleId)
        => HandleResult(await _mediator.Send(new UnassignRoleCommand(userRoleId)));

    // Role-Permission Assignment
    [HttpPost("role-permissions/{roleId:long}")]
    [AllowAnonymous] // TODO: [HasPermission("role.update")]
    public async Task<IActionResult> AssignPermissionsToRole(long roleId, [FromBody] List<long> permissionIds)
        => HandleResult(await _mediator.Send(new AssignPermissionsToRoleCommand(roleId, permissionIds)));

    [HttpDelete("role-permissions/{roleId:long}")]
    [AllowAnonymous] // TODO: [HasPermission("role.update")]
    public async Task<IActionResult> RemovePermissionsFromRole(long roleId, [FromBody] List<long> permissionIds)
        => HandleResult(await _mediator.Send(new RemovePermissionsFromRoleCommand(roleId, permissionIds)));

    // Bulk Assignment
    [HttpPost("bulk")]
    [AllowAnonymous] // TODO: [HasPermission("role.assign")]
    public async Task<IActionResult> BulkAssignRole([FromBody] BulkAssignRoleRequest request)
    {
        // Note: We need to map BulkAssignRoleRequest to BulkAssignRoleCommand; DTO is same? We'll assume BulkAssignRoleRequest is defined.
        var command = new BulkAssignRoleCommand(
            request.TenantId,
            request.RoleId,
            request.ScopeType,
            request.ScopeValue,
            request.UserIds
        );
        return HandleResult(await _mediator.Send(command));
    }
}