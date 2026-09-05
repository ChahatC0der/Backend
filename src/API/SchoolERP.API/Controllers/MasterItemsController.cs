using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.Application.Common.DTOs;
using SchoolERP.Application.Features.Master.Commands.MasterItem.CreateMasterItem;
using SchoolERP.Application.Features.Master.DTOs;
using SchoolERP.Application.Features.Master.Queries.MasterItem.GetMasterItems;


namespace SchoolERP.API.Controllers;

[ApiController]
[Route("api/master-items")]
public class MasterItemsController : BaseApiController
{
    private readonly IMediator _mediator;

    public MasterItemsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [AllowAnonymous] // TODO: [HasPermission("master_item.read")]
    public async Task<IActionResult> GetMasterItems([FromQuery] PagedRequest request, [FromQuery] long? categoryId = null)
        => HandleResult(await _mediator.Send(new GetMasterItemsQuery(request, categoryId)));

    [HttpGet("{id:long}")]
    [AllowAnonymous] // TODO: [HasPermission("master_item.read")]
    public async Task<IActionResult> GetMasterItemById(long id)
        => HandleResult(await _mediator.Send(new GetMasterItemByIdQuery(id)));

    [HttpGet("by-category/{categoryId:long}")]
    [AllowAnonymous] // TODO: [HasPermission("master_item.read")]
    public async Task<IActionResult> GetMasterItemsLight(long categoryId)
        => HandleResult(await _mediator.Send(new GetMasterItemsLightQuery(categoryId)));

    [HttpGet("export/{categoryId:long}")]
    [AllowAnonymous] // TODO: [HasPermission("master_item.export")]
    public async Task<IActionResult> ExportMasterItems(long categoryId)
    {
        var result = await _mediator.Send(new ExportMasterItemsQuery(categoryId));
        if (result.IsFailure) return HandleResult(result);
        return File(result.Value, "text/csv", "master_items.csv");
    }

    [HttpPost]
    [AllowAnonymous] // TODO: [HasPermission("master_item.create")]
    public async Task<IActionResult> CreateMasterItem([FromBody] CreateMasterItemRequest request)
        => HandleResult(await _mediator.Send(new CreateMasterItemCommand(request)));

    [HttpPut("{id:long}")]
    [AllowAnonymous] // TODO: [HasPermission("master_item.update")]
    public async Task<IActionResult> UpdateMasterItem(long id, [FromBody] UpdateMasterItemRequest request)
    {
        if (id != request.Id) return BadRequest("Id mismatch.");
        return HandleResult(await _mediator.Send(new UpdateMasterItemCommand(request)));
    }

    [HttpPatch("{id:long}")]
    [AllowAnonymous] // TODO: [HasPermission("master_item.update")]
    public async Task<IActionResult> PatchMasterItem(long id, [FromBody] PatchMasterItemRequest request)
    {
        if (id != request.Id) return BadRequest("Id mismatch.");
        return HandleResult(await _mediator.Send(new PatchMasterItemCommand(request)));
    }

    [HttpDelete("{id:long}")]
    [AllowAnonymous] // TODO: [HasPermission("master_item.delete")]
    public async Task<IActionResult> DeleteMasterItem(long id)
        => HandleResult(await _mediator.Send(new DeleteMasterItemCommand(id)));

    [HttpPost("{id:long}/restore")]
    [AllowAnonymous] // TODO: [HasPermission("master_item.update")]
    public async Task<IActionResult> RestoreMasterItem(long id)
        => HandleResult(await _mediator.Send(new RestoreMasterItemCommand(id)));

    [HttpPut("bulk")]
    [AllowAnonymous] // TODO: [HasPermission("master_item.update")]
    public async Task<IActionResult> BulkUpdateMasterItem([FromBody] BulkUpdateMasterItemRequest request)
        => HandleResult(await _mediator.Send(new BulkUpdateMasterItemCommand(request)));

    [HttpPatch("bulk")]
    [AllowAnonymous] // TODO: [HasPermission("master_item.update")]
    public async Task<IActionResult> BulkPatchMasterItem([FromBody] BulkPatchMasterItemRequest request)
        => HandleResult(await _mediator.Send(new BulkPatchMasterItemCommand(request)));

    [HttpDelete("bulk")]
    [AllowAnonymous] // TODO: [HasPermission("master_item.delete")]
    public async Task<IActionResult> BulkDeleteMasterItem([FromBody] BulkDeleteMasterItemRequest request)
        => HandleResult(await _mediator.Send(new BulkDeleteMasterItemCommand(request)));
}