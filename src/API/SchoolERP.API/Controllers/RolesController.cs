using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.API.Authorization;
using SchoolERP.Application.Features.Rbac.Commands.Role.CreateRole;
using SchoolERP.Application.Features.Rbac.DTOs;
using SchoolERP.Application.Features.Rbac.Queries.Role.GetAllRolesLight;
using SchoolERP.Application.Features.Rbac.Queries.Role.GetRoleById;
using SchoolERP.Application.Features.Rbac.Queries.Role.GetRoles;

namespace SchoolERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RolesController : BaseApiController
{
    private readonly IMediator _mediator;

    public RolesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [HasPermission("role.read")]
    public async Task<IActionResult> GetRoles([FromQuery] GetRolesRequest request)
        => HandleResult(await _mediator.Send(new GetRolesQuery(request)));

    [HttpGet("{id:long}")]
    [AllowAnonymous] // TODO: [HasPermission("role.read")]
    public async Task<IActionResult> GetRoleById(long id)
        => HandleResult(await _mediator.Send(new GetRoleByIdQuery(id)));

    [HttpGet("all")]
    [AllowAnonymous] // TODO: [HasPermission("role.read")]
    public async Task<IActionResult> GetAllRolesLight()
        => HandleResult(await _mediator.Send(new GetAllRolesLightQuery()));

    [HttpPost]
    [AllowAnonymous] // TODO: [HasPermission("role.create")]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequest request)
        => HandleResult(await _mediator.Send(new CreateRoleCommand(request)));

    [HttpPut("{id:long}")]
    [AllowAnonymous] // TODO: [HasPermission("role.update")]
    public async Task<IActionResult> UpdateRole(long id, [FromBody] UpdateRoleRequest request)
    {

        return HandleResult(await _mediator.Send(new UpdateRoleCommand(id,request)));
    }

    [HttpDelete("{id:long}")]
    [AllowAnonymous] // TODO: [HasPermission("role.delete")]
    public async Task<IActionResult> DeleteRole(long id)
        => HandleResult(await _mediator.Send(new DeleteRoleCommand(id)));
}