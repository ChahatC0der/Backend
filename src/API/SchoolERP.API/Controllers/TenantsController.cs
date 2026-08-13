using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.API.Authorization;
using SchoolERP.Application.Features.Tenants.Commands.CreateTenant;
using SchoolERP.Application.Features.Tenants.DTOs;

namespace SchoolERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TenantsController : BaseApiController
{
    [HttpPost]
    //[HasPermission("tenant.create")] // Super Admin only (Phase 5)
    [AllowAnonymous]
    public async Task<IActionResult> Create([FromBody] CreateTenantRequest request)
    {
        var result = await Mediator.Send(new CreateTenantCommand(request));
        return HandleResult(result);
    }
}