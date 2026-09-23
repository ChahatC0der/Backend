using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.Application.Features.AI.Commands.Chat;
using SchoolERP.Application.Features.AI.Commands.SemanticChat;
using SchoolERP.Application.Features.AI.DTOs;

namespace SchoolERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AIController : BaseApiController
{
    [HttpPost("chat")]
    [AllowAnonymous]
    // TODO: [HasPermission("AI.Chat")]
    public async Task<IActionResult> Chat(
        [FromBody] AiChatRequest request,
        CancellationToken cancellationToken)
    {
        return HandleResult(
            await Mediator.Send(
                new ChatAiCommand(request),
                cancellationToken));
    }

    [HttpPost("semantic-chat")]
    [AllowAnonymous]
    // TODO: [HasPermission("AI.Chat")]
    public async Task<IActionResult> SemanticChat(
    [FromBody] AiChatRequest request,
    CancellationToken cancellationToken)
    {
        return HandleResult(
            await Mediator.Send(
                new SemanticChatAiCommand(request),
                cancellationToken));
    }
}