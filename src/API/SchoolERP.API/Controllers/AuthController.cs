using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Auth.Commands.Login;
using SchoolERP.Application.Features.Auth.Queries.GetMe;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public AuthController(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginCommand command)
        => HandleResult(await _mediator.Send(command));

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMe(CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserId();

        if (userId is null)
            return Fail(Error.Unauthorized("User not authenticated."));

        return HandleResult(await _mediator.Send(new GetMeQuery(userId.Value), cancellationToken));
    }
}