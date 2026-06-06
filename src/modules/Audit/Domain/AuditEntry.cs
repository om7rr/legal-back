using LegalPlatform.SharedKernel.Classification;
using LegalPlatform.SharedKernel.Multitenancy;

namespace LegalPlatform.Modules.Audit.Domain;

/// <summary>
/// One append-only, tamper-evident audit record. Entries are hash-chained per tenant
/// (<see cref="PreviousHash"/> → <see cref="Hash"/>) for legal defensibility. Never stores raw
/// secrets or unredacted privileged content — only references (<see cref="EntityType"/>/<see cref="EntityId"/>).
/// </summary>
[DataClassification(DataClassification.Confidential)]
public sealed class AuditEntry : ITenantOwned
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    /// <summary>Owning tenant. Enforced by the global query filter + Postgres RLS.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>What happened, e.g. "invoice.issued", "case.filed".</summary>
    public string Action { get; private set; } = string.Empty;

    /// <summary>The affected entity type, e.g. "Invoice".</summary>
    public string EntityType { get; private set; } = string.Empty;

    /// <summary>Reference to the affected entity (id), never its contents.</summary>
    public string EntityId { get; private set; } = string.Empty;

    /// <summary>Who triggered the action (user id), or null for system actions.</summary>
    public Guid? ActorId { get; private set; }

    public DateTimeOffset OccurredAt { get; private set; } = DateTimeOffset.UtcNow;

    /// <summary>Hash of the previous entry in this tenant's chain (empty for the first).</summary>
    public string PreviousHash { get; private set; } = string.Empty;

    /// <summary>Tamper-evident hash of this entry chained to <see cref="PreviousHash"/>.</summary>
    public string Hash { get; private set; } = string.Empty;

    private AuditEntry() { }

    public AuditEntry(
        Guid tenantId,
        string action,
        string entityType,
        string entityId,
        Guid? actorId,
        string previousHash,
        string hash)
    {
        TenantId = tenantId;
        Action = action;
        EntityType = entityType;
        EntityId = entityId;
        ActorId = actorId;
        PreviousHash = previousHash;
        Hash = hash;
    }
}
