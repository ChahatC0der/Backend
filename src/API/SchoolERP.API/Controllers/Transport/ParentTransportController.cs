using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.Application.Features.Transport.Queries.Portal.GetParentChildTransportView;

namespace SchoolERP.API.Controllers.Transport;

/// <summary>
/// TEMPORARY: [AllowAnonymous]. When RBAC ready, replace with
/// [HasPermission(PermissionNames.TransportPortalRead)].
/// Parent-child auth enforced INSIDE handler via ParentChildResolver regardless.
/// </summary>
[ApiController]
[Route("api/portal/transport")]
[AllowAnonymous]
public sealed class ParentTransportController : BaseApiController
{
    private readonly IMediator _mediator;
    public ParentTransportController(IMediator mediator) => _mediator = mediator;

    [HttpGet("children/{studentId:long}")]
    [AllowAnonymous]  // TODO: [HasPermission(PermissionNames.TransportPortalRead)]
    public async Task<IActionResult> GetChildView([FromRoute] long studentId, [FromQuery] long? academicYearId, CancellationToken ct)
        => HandleResult(await _mediator.Send(new GetParentChildTransportViewQuery(studentId, academicYearId), ct));
}
