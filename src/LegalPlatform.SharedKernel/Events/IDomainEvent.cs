namespace LegalPlatform.SharedKernel.Events;

/// <summary>
/// A fact that happened inside a bounded context. Raised and handled in-process.
/// Cross-context communication uses <see cref="IIntegrationEvent"/> instead.
/// </summary>
public interface IDomainEvent
{
    Guid EventId { get; }
    DateTimeOffset OccurredAt { get; }
}
