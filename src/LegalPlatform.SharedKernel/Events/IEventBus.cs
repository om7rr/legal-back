namespace LegalPlatform.SharedKernel.Events;

/// <summary>
/// Publishes integration events to other bounded contexts. The concrete implementation lives in
/// Infrastructure (outbox-backed, idempotent retries). Contexts depend only on this abstraction.
/// </summary>
public interface IEventBus
{
    Task PublishAsync(IIntegrationEvent integrationEvent, CancellationToken cancellationToken = default);
}
