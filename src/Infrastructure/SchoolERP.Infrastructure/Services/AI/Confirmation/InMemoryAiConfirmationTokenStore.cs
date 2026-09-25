using System.Collections.Concurrent;
using SchoolERP.Application.Features.AI.Confirmation;

namespace SchoolERP.Infrastructure.Services.AI.Confirmation;

public sealed class InMemoryAiConfirmationTokenStore
    : IAiConfirmationTokenStore
{
    private readonly ConcurrentDictionary<
        Guid,
        DateTimeOffset> _consumedTokens = new();

    public Task<bool> TryConsumeAsync(
        Guid confirmationId,
        DateTimeOffset expiresAtUtc,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (confirmationId == Guid.Empty)
            return Task.FromResult(false);

        if (expiresAtUtc <= DateTimeOffset.UtcNow)
            return Task.FromResult(false);

        CleanupExpiredEntries();

        var consumed =
            _consumedTokens.TryAdd(
                confirmationId,
                expiresAtUtc);

        return Task.FromResult(consumed);
    }

    private void CleanupExpiredEntries()
    {
        var now =
            DateTimeOffset.UtcNow;

        foreach (var entry in _consumedTokens)
        {
            if (entry.Value <= now)
            {
                _consumedTokens.TryRemove(
                    entry.Key,
                    out _);
            }
        }
    }
}