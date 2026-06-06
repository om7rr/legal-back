using LegalPlatform.SharedKernel.Events;
using Microsoft.Extensions.DependencyInjection;

namespace LegalPlatform.Infrastructure.Events;

/// <summary>
/// In-process event bus (ADR-0005): dispatches an integration event to every registered
/// <see cref="IIntegrationEventHandler{TEvent}"/> synchronously, within the current scope. A
/// transactional outbox replaces this before any event has external side effects.
/// </summary>
public sealed class InProcessEventBus : IEventBus
{
    private readonly IServiceProvider _serviceProvider;

    public InProcessEventBus(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

    public async Task PublishAsync(IIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(integrationEvent);

        var handlerType = typeof(IIntegrationEventHandler<>).MakeGenericType(integrationEvent.GetType());
        var method = handlerType.GetMethod(nameof(IIntegrationEventHandler<IIntegrationEvent>.HandleAsync))!;

        foreach (var handler in _serviceProvider.GetServices(handlerType))
        {
            if (handler is null)
            {
                continue;
            }

            await (Task)method.Invoke(handler, [integrationEvent, cancellationToken])!;
        }
    }
}
