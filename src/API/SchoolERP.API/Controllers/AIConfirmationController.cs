using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.API.Controllers;
using SchoolERP.Application.Features.AI.Commands.Confirmation;
using SchoolERP.Application.Features.AI.DTOs;

namespace SchoolERP.API.Controllers;

[ApiController]
[Route("api/ai")]
public sealed class AIConfirmationController
    : BaseApiController
{
    [HttpPost("confirm")]
    [Authorize]
    // TODO: Add the final AI confirmation permission
    // once the application's permission claim source
    // is finalized.
    public async Task<IActionResult> Confirm(
        [FromBody] ConfirmAiActionRequest request,
        CancellationToken cancellationToken)
    {
        var result =
            await Mediator.Send(
                new ConfirmAiActionCommand(
                    request.ConfirmationToken),
                cancellationToken);

        return HandleResult(result);
    }
}