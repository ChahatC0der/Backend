using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.AI.DTOs;

namespace SchoolERP.UnitTests.AI.Fakes;

public sealed class FakeAiGateway : IAiGateway
{
    private readonly AiChatResponse _response;

    public FakeAiGateway(AiChatResponse response)
    {
        _response = response;
    }

    public AiChatRequest? LastRequest { get; private set; }

    public Task<AiChatResponse> ChatAsync(
        AiChatRequest request,
        CancellationToken cancellationToken = default)
    {
        LastRequest = request;

        return Task.FromResult(_response);
    }

    public IAsyncEnumerable<AiStreamChunk> StreamChatAsync(
        AiChatRequest request,
        CancellationToken cancellationToken = default)
    {
        LastRequest = request;

        return EmptyStream();
    }

    private static async IAsyncEnumerable<AiStreamChunk> EmptyStream()
    {
        await Task.CompletedTask;
        yield break;
    }
}