using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.Application.Common.DTOs;
using SchoolERP.Application.Features.Academic.Commands.AcademicYear.CreateAcademicYear;
using SchoolERP.Application.Features.Academic.DTOs;
using SchoolERP.Application.Features.Academic.Queries.AcademicYear.ExportAcademicYears;
using SchoolERP.Application.Features.Academic.Queries.AcademicYear.GetAcademicYearById;
using SchoolERP.Application.Features.Academic.Queries.AcademicYear.GetAcademicYears;
using SchoolERP.Application.Features.Academic.Queries.AcademicYear.GetAcademicYearsLight;

namespace SchoolERP.API.Controllers;

[ApiController]
[Route("api/academic-years")]
public class AcademicYearsController : BaseApiController
{
    private readonly IMediator _mediator;

    public AcademicYearsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [AllowAnonymous] // TODO: [HasPermission("academic_year.read")]
    public async Task<IActionResult> GetAcademicYears([FromQuery] PagedRequest request)
        => HandleResult(await _mediator.Send(new GetAcademicYearsQuery(request)));

    [HttpGet("{id:long}")]
    [AllowAnonymous] // TODO: [HasPermission("academic_year.read")]
    public async Task<IActionResult> GetAcademicYearById(long id)
        => HandleResult(await _mediator.Send(new GetAcademicYearByIdQuery(id)));

    [HttpGet("all")]
    [AllowAnonymous] // TODO: [HasPermission("academic_year.read")]
    public async Task<IActionResult> GetAcademicYearsLight()
        => HandleResult(await _mediator.Send(new GetAcademicYearsLightQuery()));

    [HttpGet("export")]
    [AllowAnonymous] // TODO: [HasPermission("academic_year.export")]
    public async Task<IActionResult> ExportAcademicYears()
    {
        var result = await _mediator.Send(new ExportAcademicYearsQuery());
        if (result.IsFailure) return HandleResult(result);
        return File(result.Value, "text/csv", "academic_years.csv");
    }

    [HttpPost]
    [AllowAnonymous] // TODO: [HasPermission("academic_year.create")]
    public async Task<IActionResult> CreateAcademicYear([FromBody] CreateAcademicYearRequest request)
        => HandleResult(await _mediator.Send(new CreateAcademicYearCommand(request)));

    [HttpPut("{id:long}")]
    [AllowAnonymous] // TODO: [HasPermission("academic_year.update")]
    public async Task<IActionResult> UpdateAcademicYear(long id, [FromBody] UpdateAcademicYearRequest request)
    {
        if (id != request.Id) return BadRequest("Id mismatch.");
        return HandleResult(await _mediator.Send(new UpdateAcademicYearCommand(request)));
    }

    [HttpPatch("{id:long}")]
    [AllowAnonymous] // TODO: [HasPermission("academic_year.update")]
    public async Task<IActionResult> PatchAcademicYear(long id, [FromBody] PatchAcademicYearRequest request)
    {
        if (id != request.Id) return BadRequest("Id mismatch.");
        return HandleResult(await _mediator.Send(new PatchAcademicYearCommand(request)));
    }

    [HttpDelete("{id:long}")]
    [AllowAnonymous] // TODO: [HasPermission("academic_year.delete")]
    public async Task<IActionResult> DeleteAcademicYear(long id)
        => HandleResult(await _mediator.Send(new DeleteAcademicYearCommand(id)));

    [HttpPost("{id:long}/restore")]
    [AllowAnonymous] // TODO: [HasPermission("academic_year.update")]
    public async Task<IActionResult> RestoreAcademicYear(long id)
        => HandleResult(await _mediator.Send(new RestoreAcademicYearCommand(id)));

    [HttpPut("bulk")]
    [AllowAnonymous] // TODO: [HasPermission("academic_year.update")]
    public async Task<IActionResult> BulkUpdateAcademicYear([FromBody] BulkUpdateAcademicYearRequest request)
        => HandleResult(await _mediator.Send(new BulkUpdateAcademicYearCommand(request)));

    [HttpPatch("bulk")]
    [AllowAnonymous] // TODO: [HasPermission("academic_year.update")]
    public async Task<IActionResult> BulkPatchAcademicYear([FromBody] BulkPatchAcademicYearRequest request)
        => HandleResult(await _mediator.Send(new BulkPatchAcademicYearCommand(request)));

    [HttpDelete("bulk")]
    [AllowAnonymous] // TODO: [HasPermission("academic_year.delete")]
    public async Task<IActionResult> BulkDeleteAcademicYear([FromBody] BulkDeleteAcademicYearRequest request)
        => HandleResult(await _mediator.Send(new BulkDeleteAcademicYearCommand(request)));
}