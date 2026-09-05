using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.Application.Common.DTOs;
using SchoolERP.Application.Features.Master.Commands.MasterCategory.CreateMasterCategory;
using SchoolERP.Application.Features.Master.DTOs;
using SchoolERP.Application.Features.Master.Queries.MasterCategory.GetMasterCategories;


namespace SchoolERP.API.Controllers;

[ApiController]
[Route("api/master-categories")]
public class MasterCategoriesController : BaseApiController
{
    private readonly IMediator _mediator;

    public MasterCategoriesController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [AllowAnonymous] // TODO: [HasPermission("master_category.read")]
    public async Task<IActionResult> GetMasterCategories([FromQuery] PagedRequest request)
        => HandleResult(await _mediator.Send(new GetMasterCategoriesQuery(request)));

    [HttpGet("{id:long}")]
    [AllowAnonymous] // TODO: [HasPermission("master_category.read")]
    public async Task<IActionResult> GetMasterCategoryById(long id)
        => HandleResult(await _mediator.Send(new GetMasterCategoryByIdQuery(id)));

    [HttpGet("all")]
    [AllowAnonymous] // TODO: [HasPermission("master_category.read")]
    public async Task<IActionResult> GetMasterCategoriesLight()
        => HandleResult(await _mediator.Send(new GetMasterCategoriesLightQuery()));

    [HttpGet("export")]
    [AllowAnonymous] // TODO: [HasPermission("master_category.export")]
    public async Task<IActionResult> ExportMasterCategories()
    {
        var result = await _mediator.Send(new ExportMasterCategoriesQuery());
        if (result.IsFailure) return HandleResult(result);
        return File(result.Value, "text/csv", "master_categories.csv");
    }

    [HttpPost]
    [AllowAnonymous] // TODO: [HasPermission("master_category.create")]
    public async Task<IActionResult> CreateMasterCategory([FromBody] CreateMasterCategoryRequest request)
        => HandleResult(await _mediator.Send(new CreateMasterCategoryCommand(request)));

    [HttpPut("{id:long}")]
    [AllowAnonymous] // TODO: [HasPermission("master_category.update")]
    public async Task<IActionResult> UpdateMasterCategory(long id, [FromBody] UpdateMasterCategoryRequest request)
    {
        if (id != request.Id) return BadRequest("Id mismatch.");
        return HandleResult(await _mediator.Send(new UpdateMasterCategoryCommand(request)));
    }

    [HttpPatch("{id:long}")]
    [AllowAnonymous] // TODO: [HasPermission("master_category.update")]
    public async Task<IActionResult> PatchMasterCategory(long id, [FromBody] PatchMasterCategoryRequest request)
    {
        if (id != request.Id) return BadRequest("Id mismatch.");
        return HandleResult(await _mediator.Send(new PatchMasterCategoryCommand(request)));
    }

    [HttpDelete("{id:long}")]
    [AllowAnonymous] // TODO: [HasPermission("master_category.delete")]
    public async Task<IActionResult> DeleteMasterCategory(long id)
        => HandleResult(await _mediator.Send(new DeleteMasterCategoryCommand(id)));

    [HttpPost("{id:long}/restore")]
    [AllowAnonymous] // TODO: [HasPermission("master_category.update")]
    public async Task<IActionResult> RestoreMasterCategory(long id)
        => HandleResult(await _mediator.Send(new RestoreMasterCategoryCommand(id)));

    [HttpPut("bulk")]
    [AllowAnonymous] // TODO: [HasPermission("master_category.update")]
    public async Task<IActionResult> BulkUpdateMasterCategory([FromBody] BulkUpdateMasterCategoryRequest request)
        => HandleResult(await _mediator.Send(new BulkUpdateMasterCategoryCommand(request)));

    [HttpPatch("bulk")]
    [AllowAnonymous] // TODO: [HasPermission("master_category.update")]
    public async Task<IActionResult> BulkPatchMasterCategory([FromBody] BulkPatchMasterCategoryRequest request)
        => HandleResult(await _mediator.Send(new BulkPatchMasterCategoryCommand(request)));

    [HttpDelete("bulk")]
    [AllowAnonymous] // TODO: [HasPermission("master_category.delete")]
    public async Task<IActionResult> BulkDeleteMasterCategory([FromBody] BulkDeleteMasterCategoryRequest request)
        => HandleResult(await _mediator.Send(new BulkDeleteMasterCategoryCommand(request)));
}