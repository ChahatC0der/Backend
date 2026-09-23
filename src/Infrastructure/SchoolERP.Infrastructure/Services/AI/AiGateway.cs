using Microsoft.Extensions.Options;
using SchoolERP.Application.AI.Configuration;
using SchoolERP.Application.Features.AI.DTOs;
using SchoolERP.Application.Common.Interfaces;

namespace SchoolERP.Infrastructure.AI;

public sealed class AiGateway : IAiGateway
{
    private readonly IEnumerable<IAiProvider> _providers;
    private readonly AiOptions _options;

    public AiGateway(
        IEnumerable<IAiProvider> providers,
        IOptions<AiOptions> options)
    {
        _providers = providers;
        _options = options.Value;
    }

    public async Task<AiChatResponse> ChatAsync(
        AiChatRequest request,
        CancellationToken cancellationToken = default)
    {
        EnsureEnabled();

        var provider = ResolveProvider();

        return await provider.ChatAsync(
            request,
            cancellationToken);
    }

    public IAsyncEnumerable<AiStreamChunk> StreamChatAsync(
        AiChatRequest request,
        CancellationToken cancellationToken = default)
    {
        EnsureEnabled();

        var provider = ResolveProvider();

        return provider.StreamChatAsync(
            request,
            cancellationToken);
    }

    private IAiProvider ResolveProvider()
    {
        if (string.IsNullOrWhiteSpace(_options.DefaultProvider))
        {
            throw new InvalidOperationException(
                "AI default provider is not configured.");
        }

        var provider = _providers.FirstOrDefault(
            x => string.Equals(
                x.Name,
                _options.DefaultProvider,
                StringComparison.OrdinalIgnoreCase));

        if (provider is null)
        {
            throw new InvalidOperationException(
                $"AI provider '{_options.DefaultProvider}' is not registered.");
        }

        return provider;
    }

    private void EnsureEnabled()
    {
        if (!_options.Enabled)
        {
            throw new InvalidOperationException(
                "AI is currently disabled.");
        }
    }
}