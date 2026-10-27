using MediatR;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.API.Authorization;
using SchoolERP.Application.Features.Fees.Commands.Portal.OptInOptionalFee;
using SchoolERP.Application.Features.Fees.DTOs.Portal;
using SchoolERP.Application.Features.Fees.Queries.Portal.GetAvailableOptionalFees;
using SchoolERP.Application.Features.Fees.Queries.Portal.GetParentChildFeeView;
using SchoolERP.Domain.Shared.Permissions;

namespace SchoolERP.API.Controllers.Fees;

[ApiController]
[Route("api/portal/fees")]
public sealed class ParentFeesController : BaseApiController
{
    private readonly IMediator _mediator;
    public ParentFeesController(IMediator mediator) => _mediator = mediator;

    [HttpGet("children/{studentId:long}")]
    [HasPermission(PermissionNames.FeePortalRead)]
    public async Task<IActionResult> GetChildView([FromRoute] long studentId, [FromQuery] long? academicYearId, CancellationToken ct)
        => HandleResult(await _mediator.Send(new GetParentChildFeeViewQuery(studentId, academicYearId), ct));

    [HttpGet("optional-fees/{studentId:long}")]
    [HasPermission(PermissionNames.FeePortalRead)]
    public async Task<IActionResult> GetOptionalFees([FromRoute] long studentId, [FromQuery] long academicYearId, CancellationToken ct)
        => HandleResult(await _mediator.Send(new GetAvailableOptionalFeesQuery(studentId, academicYearId), ct));

    [HttpPost("optional-fees/opt-in")]
    [HasPermission(PermissionNames.FeePortalOptIn)]
    public async Task<IActionResult> OptIn([FromBody] OptInOptionalFeeRequest request, CancellationToken ct)
        => HandleResult(await _mediator.Send(new OptInOptionalFeeCommand(request), ct));
}
