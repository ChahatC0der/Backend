using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.Application.Features.Rbac.Commands.User.DeleteUser;
using SchoolERP.Application.Features.Rbac.Commands.User.UpdateUser;
using SchoolERP.Application.Features.Rbac.DTOs;
using SchoolERP.Application.Features.Rbac.Queries.User.GetUserPermissions;
using SchoolERP.Application.Features.Rbac.Queries.User.GetUserRoles;

namespace SchoolERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : BaseApiController
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [AllowAnonymous] // TODO: [HasPermission("user.create")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
        => HandleResult(await _mediator.Send(new CreateUserCommand(request)));

    [HttpPut("{id:long}")]
    [AllowAnonymous] // TODO: [HasPermission("user.update")]
    public async Task<IActionResult> UpdateUser(long id, [FromBody] UpdateUserRequest request)
    {
        if (id != request.Id)
            return BadRequest("Id in route does not match Id in body.");

        return HandleResult(await _mediator.Send(new UpdateUserCommand(request)));
    }

    [HttpDelete("{id:long}")]
    [AllowAnonymous] // TODO: [HasPermission("user.delete")]
    public async Task<IActionResult> DeleteUser(long id)
        => HandleResult(await _mediator.Send(new DeleteUserCommand(id)));

    [HttpGet("{id:long}/permissions")]
    [AllowAnonymous] // TODO: [HasPermission("user.read")]
    public async Task<IActionResult> GetUserPermissions(long id)
        => HandleResult(await _mediator.Send(new GetUserPermissionsQuery(id)));

    [HttpGet("{id:long}/roles")]
    [AllowAnonymous] // TODO: [HasPermission("user.read")]
    public async Task<IActionResult> GetUserRoles(long id)
        => HandleResult(await _mediator.Send(new GetUserRolesQuery(id)));
}