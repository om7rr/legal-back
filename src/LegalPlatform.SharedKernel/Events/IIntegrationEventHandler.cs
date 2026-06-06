namespace LegalPlatform.SharedKernel.Events;

/// <summary>
/// Handles a specific <see cref="IIntegrationEvent"/>. Implemented by a consuming bounded context;
/// dispatched by <see cref="IEventBus"/>. Publisher and consumer reference only SharedKernel.
/// </summary>
public interface IIntegrationEventHandler<in TEvent>
    where TEvent : IIntegrationEvent
{
    Task HandleAsync(TEvent integrationEvent, CancellationToken cancellationToken = default);
}
