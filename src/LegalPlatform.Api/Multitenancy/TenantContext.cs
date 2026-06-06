using LegalPlatform.SharedKernel.Multitenancy;

namespace LegalPlatform.Api.Multitenancy;

/// <summary>
/// Request-scoped tenant context. Populated by <see cref="Middleware.TenantResolverMiddleware"/> and
/// consumed by the persistence layer's global query filter.
/// </summary>
public sealed class TenantContext : ITenantContext
{
    public Guid TenantId { get; private set; }

    public bool HasTenant { get; private set; }

    public void SetTenant(Guid tenantId)
    {
        TenantId = tenantId;
        HasTenant = tenantId != Guid.Empty;
    }
}
