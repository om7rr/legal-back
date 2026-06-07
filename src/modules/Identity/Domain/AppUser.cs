using LegalPlatform.SharedKernel.Classification;

namespace LegalPlatform.Modules.Identity.Domain;

/// <summary>
/// A platform user identified by national id (verified via Nafath). Belongs to one firm (tenant) and
/// carries a role. NOT <c>ITenantOwned</c> on purpose: login resolves the user before any tenant
/// context exists, so this entity is not subject to the global tenant query filter — user queries
/// must scope by <see cref="TenantId"/> explicitly.
/// </summary>
[DataClassification(DataClassification.RegulatedPersonalData)]
public sealed class AppUser
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string NationalId { get; private set; } = string.Empty;
    public string FullName { get; private set; } = string.Empty;
    public Guid TenantId { get; private set; }
    public UserRole Role { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;

    private AppUser() { }

    public AppUser(string nationalId, string fullName, Guid tenantId, UserRole role)
    {
        if (string.IsNullOrWhiteSpace(nationalId))
        {
            throw new ArgumentException("National id is required.", nameof(nationalId));
        }

        if (tenantId == Guid.Empty)
        {
            throw new ArgumentException("Tenant is required.", nameof(tenantId));
        }

        NationalId = nationalId.Trim();
        FullName = (fullName ?? string.Empty).Trim();
        TenantId = tenantId;
        Role = role;
    }
}
