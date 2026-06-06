using LegalPlatform.SharedKernel.Classification;

namespace LegalPlatform.Modules.Tenancy.Domain;

/// <summary>
/// A law firm using the platform. The root of tenant isolation — every other tenant-owned
/// entity references a <see cref="Tenant"/> by id.
/// </summary>
[DataClassification(DataClassification.Confidential)]
public sealed class Tenant
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    /// <summary>Display name of the firm.</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>URL-safe unique identifier for the firm.</summary>
    public string Slug { get; private set; } = string.Empty;

    public bool IsActive { get; private set; } = true;

    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;

    private Tenant() { }

    public Tenant(string name, string slug)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Tenant name is required.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(slug))
        {
            throw new ArgumentException("Tenant slug is required.", nameof(slug));
        }

        Name = name.Trim();
        Slug = slug.Trim().ToLowerInvariant();
    }

    public void Deactivate() => IsActive = false;
}
