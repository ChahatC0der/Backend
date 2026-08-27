using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.Application.Features.Rbac.Queries.Permission.GetPermissions;

namespace SchoolERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PermissionsController : BaseApiController
{
    private readonly IMediator _mediator;

    public PermissionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [AllowAnonymous] // TODO: [HasPermission("permission.read")]
    public async Task<IActionResult> GetPermissions()
        => HandleResult(await _mediator.Send(new GetPermissionsQuery()));
}