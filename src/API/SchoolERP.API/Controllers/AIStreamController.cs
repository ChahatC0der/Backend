using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.Application.Features.AI.DTOs;
using SchoolERP.Application.Features.AI.Streaming;

namespace SchoolERP.API.Controllers;

[ApiController]
[Route("api/ai")]
public sealed class AIStreamController : BaseApiController
{
    private readonly IAiStreamService _streamService;
    private readonly IAiAgentStreamService _agentStreamService;

    public AIStreamController(
        IAiStreamService streamService,
        IAiAgentStreamService agentStreamService)
    {
        _streamService = streamService;
        _agentStreamService = agentStreamService;
    }

    [HttpPost("stream")]
    [Authorize]
    public async Task Stream(
        [FromBody] AiChatRequest request,
        CancellationToken cancellationToken)
    {
        await WriteStreamAsync(
            _streamService.StreamAsync(
                request,
                cancellationToken),
            cancellationToken);
    }

    [HttpPost("agent/stream")]
    [Authorize]
    public async Task AgentStream(
        [FromBody] AiAgentRequest request,
        CancellationToken cancellationToken)
    {
        await WriteStreamAsync(
            _agentStreamService.StreamAsync(
                request,
                cancellationToken),
            cancellationToken);
    }

    private async Task WriteStreamAsync(
        IAsyncEnumerable<AiStreamEvent> stream,
        CancellationToken cancellationToken)
    {
        Response.StatusCode = StatusCodes.Status200OK;
        Response.ContentType = "text/event-stream; charset=utf-8";
        Response.Headers.CacheControl = "no-cache, no-store";
        Response.Headers.Append("X-Accel-Buffering", "no");
        Response.Headers.Append("X-Content-Type-Options", "nosniff");

        try
        {
            await foreach (var streamEvent in stream)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var json = JsonSerializer.Serialize(streamEvent);

                await Response.WriteAsync(
                    $"event: {streamEvent.Type}\n",
                    cancellationToken);

                await Response.WriteAsync(
                    $"data: {json}\n\n",
                    cancellationToken);

                await Response.Body.FlushAsync(
                    cancellationToken);
            }
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            // Client disconnected/request cancelled.
        }
    }
}