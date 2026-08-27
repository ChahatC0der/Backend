using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc; 
using SchoolERP.Application.Features.Rbac.DTOs;
using SchoolERP.Application.Features.Rbac.Queries.AuditLog.GetRbacAuditLogs;

namespace SchoolERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuditLogsController : BaseApiController
{
    private readonly IMediator _mediator;
    public AuditLogsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [AllowAnonymous] // TODO: [HasPermission("audit.read")]
    public async Task<IActionResult> GetAuditLogs([FromQuery] GetRbacAuditLogsRequest request)
        => HandleResult(await _mediator.Send(new GetRbacAuditLogsQuery(request)));
}