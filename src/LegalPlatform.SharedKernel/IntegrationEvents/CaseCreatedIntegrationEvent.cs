using LegalPlatform.SharedKernel.Events;

namespace LegalPlatform.SharedKernel.IntegrationEvents;

/// <summary>
/// Raised by the Cases context when a case is created. Lives in SharedKernel (shared contract) so the
/// Audit context can consume it without referencing the Cases module (ADR-0005).
/// </summary>
public sealed record CaseCreatedIntegrationEvent(
    Guid CaseId,
    Guid TenantId,
    string CaseNumber,
    Guid? ActorId) : IIntegrationEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
