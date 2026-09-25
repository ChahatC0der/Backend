using FluentAssertions;
using SchoolERP.Infrastructure.Services.AI.Confirmation;
using Xunit;

namespace SchoolERP.UnitTests.Features.AI.Confirmation;

public sealed class InMemoryAiConfirmationTokenStoreTests
{
    [Fact]
    public async Task Should_consume_token_only_once()
    {
        var store =
            new InMemoryAiConfirmationTokenStore();

        var confirmationId =
            Guid.NewGuid();

        var expiresAt =
            DateTimeOffset.UtcNow.AddMinutes(5);

        var first =
            await store.TryConsumeAsync(
                confirmationId,
                expiresAt);

        var second =
            await store.TryConsumeAsync(
                confirmationId,
                expiresAt);

        first.Should().BeTrue();
        second.Should().BeFalse();
    }

    [Fact]
    public async Task Should_allow_different_confirmation_ids()
    {
        var store =
            new InMemoryAiConfirmationTokenStore();

        var expiresAt =
            DateTimeOffset.UtcNow.AddMinutes(5);

        var first =
            await store.TryConsumeAsync(
                Guid.NewGuid(),
                expiresAt);

        var second =
            await store.TryConsumeAsync(
                Guid.NewGuid(),
                expiresAt);

        first.Should().BeTrue();
        second.Should().BeTrue();
    }

    [Fact]
    public async Task Should_reject_empty_confirmation_id()
    {
        var store =
            new InMemoryAiConfirmationTokenStore();

        var result =
            await store.TryConsumeAsync(
                Guid.Empty,
                DateTimeOffset.UtcNow.AddMinutes(5));

        result.Should().BeFalse();
    }

    [Fact]
    public async Task Should_reject_expired_confirmation()
    {
        var store =
            new InMemoryAiConfirmationTokenStore();

        var result =
            await store.TryConsumeAsync(
                Guid.NewGuid(),
                DateTimeOffset.UtcNow.AddSeconds(-1));

        result.Should().BeFalse();
    }

    [Fact]
    public async Task Should_allow_only_one_concurrent_consumer()
    {
        var store =
            new InMemoryAiConfirmationTokenStore();

        var confirmationId =
            Guid.NewGuid();

        var expiresAt =
            DateTimeOffset.UtcNow.AddMinutes(5);

        var tasks =
            Enumerable
                .Range(0, 50)
                .Select(
                    _ =>
                        store.TryConsumeAsync(
                            confirmationId,
                            expiresAt));

        var results =
            await Task.WhenAll(tasks);

        results.Count(x => x)
            .Should()
            .Be(1);
    }

    [Fact]
    public async Task Should_honor_cancellation()
    {
        var store =
            new InMemoryAiConfirmationTokenStore();

        using var cancellationTokenSource =
            new CancellationTokenSource();

        cancellationTokenSource.Cancel();

        Func<Task> action =
            () =>
                store.TryConsumeAsync(
                    Guid.NewGuid(),
                    DateTimeOffset.UtcNow.AddMinutes(5),
                    cancellationTokenSource.Token);

        await action
            .Should()
            .ThrowAsync<OperationCanceledException>();
    }
}