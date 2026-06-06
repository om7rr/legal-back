namespace LegalPlatform.SharedKernel.Multitenancy;

/// <summary>
/// Ambient tenant for the current request/scope. Resolved by middleware and consumed by the
/// persistence layer to scope every query. Defense-in-depth alongside Postgres RLS (ADR-0002).
/// </summary>
public interface ITenantContext
{
    /// <summary>The resolved tenant id, or <see cref="Guid.Empty"/> when none is set.</summary>
    Guid TenantId { get; }

    /// <summary>True when a tenant has been resolved for the current scope.</summary>
    bool HasTenant { get; }
}
