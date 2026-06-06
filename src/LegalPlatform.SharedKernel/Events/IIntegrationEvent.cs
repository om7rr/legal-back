namespace LegalPlatform.SharedKernel.Events;

/// <summary>
/// The contract crossing bounded-context boundaries. Published via the outbox and consumed by
/// other contexts — the ONLY sanctioned cross-domain coupling (see ADR-0001). Never reach into
/// another context's tables directly.
/// </summary>
public interface IIntegrationEvent
{
    Guid EventId { get; }
    DateTimeOffset OccurredAt { get; }
}
