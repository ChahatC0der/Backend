namespace SchoolERP.Application.Features.AI.Confirmation;

public interface IAiConfirmationTokenStore
{
    Task<bool> TryConsumeAsync(
        Guid confirmationId,
        DateTimeOffset expiresAtUtc,
        CancellationToken cancellationToken = default);
}