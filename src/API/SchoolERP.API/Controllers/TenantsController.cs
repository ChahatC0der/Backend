using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.Application.Common.DTOs;
using SchoolERP.Application.Features.Tenants.Commands.BulkDelete;
using SchoolERP.Application.Features.Tenants.Commands.BulkUpdate;
using SchoolERP.Application.Features.Tenants.Commands.CreateTenant;
using SchoolERP.Application.Features.Tenants.Commands.DeleteTenant;
using SchoolERP.Application.Features.Tenants.Commands.PatchTenant;
using SchoolERP.Application.Features.Tenants.Commands.RestoreTenant;
using SchoolERP.Application.Features.Tenants.Commands.UpdateTenant;
using SchoolERP.Application.Features.Tenants.DTOs;
using SchoolERP.Application.Features.Tenants.Queries.ExportTenants;
using SchoolERP.Application.Features.Tenants.Queries.GetAllTenants;
using SchoolERP.Application.Features.Tenants.Queries.GetAllTenantsLight;
using SchoolERP.Application.Features.Tenants.Queries.GetTenantById;

namespace SchoolERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TenantsController : BaseApiController
{
    // 📌 CREATE
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Create([FromBody] CreateTenantRequest request)
        => HandleResult(await Mediator.Send(new CreateTenantCommand(request)));

    // 📌 GET ALL (PAGED)
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll([FromQuery] PagedRequest request)
        => HandleResult(await Mediator.Send(new GetAllTenantsQuery(request)));

    // 📌 GET ALL (LIGHTWEIGHT)
    [HttpGet("all")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAllLight()
        => HandleResult(await Mediator.Send(new GetAllTenantsLightQuery()));

    // 📌 GET BY ID
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id)
        => HandleResult(await Mediator.Send(new GetTenantByIdQuery(id)));

    // 📌 UPDATE (FULL)
    [HttpPut("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTenantRequest request)
        => HandleResult(await Mediator.Send(new UpdateTenantCommand(id, request)));

    // 📌 BULK UPDATE
    [HttpPut("bulk")]
    [AllowAnonymous] // TODO: [HasPermission("tenant.bulk.update")]
    public async Task<IActionResult> BulkUpdate([FromBody] BulkUpdateTenantRequest request)
        => HandleResult(await Mediator.Send(new BulkUpdateTenantCommand(request)));

    // 📌 PATCH (PARTIAL)
    [HttpPatch("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> Patch(Guid id, [FromBody] PatchTenantRequest request)
        => HandleResult(await Mediator.Send(new PatchTenantCommand(id, request)));

    // 📌 DELETE (SOFT)
    [HttpDelete("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> Delete(Guid id)
        => HandleResult(await Mediator.Send(new DeleteTenantCommand(id)));

    // 📌 RESTORE
    [HttpPost("{id:guid}/restore")]
    [AllowAnonymous]
    public async Task<IActionResult> Restore(Guid id)
        => HandleResult(await Mediator.Send(new RestoreTenantCommand(id)));

    // 📌 BULK DELETE
    [HttpDelete("bulk")]
    [AllowAnonymous]
    public async Task<IActionResult> BulkDelete([FromBody] BulkDeleteRequest request)
        => HandleResult(await Mediator.Send(new BulkDeleteTenantCommand(request)));

    // 📌 EXPORT CSV
    [HttpGet("export")]
    [AllowAnonymous]
    public async Task<IActionResult> Export()
    {
        var result = await Mediator.Send(new ExportTenantsQuery());
        if (result.IsFailure)
            return HandleResult(result);

        var fileName = $"tenants_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";
        return File(result.Value!, "text/csv", fileName);
    }
}