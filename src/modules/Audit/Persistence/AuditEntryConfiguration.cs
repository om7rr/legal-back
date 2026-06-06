using LegalPlatform.Modules.Audit.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LegalPlatform.Modules.Audit.Persistence;

/// <summary>EF mapping for <see cref="AuditEntry"/>. Owned by the Audit context.</summary>
public sealed class AuditEntryConfiguration : IEntityTypeConfiguration<AuditEntry>
{
    public void Configure(EntityTypeBuilder<AuditEntry> builder)
    {
        builder.ToTable("audit_entries", "audit");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.TenantId).IsRequired();
        builder.Property(a => a.Action).IsRequired().HasMaxLength(100);
        builder.Property(a => a.EntityType).IsRequired().HasMaxLength(100);
        builder.Property(a => a.EntityId).IsRequired().HasMaxLength(100);
        builder.Property(a => a.PreviousHash).IsRequired().HasMaxLength(128);
        builder.Property(a => a.Hash).IsRequired().HasMaxLength(128);
        builder.Property(a => a.OccurredAt).IsRequired();
        // Query path for a tenant's chain, newest first.
        builder.HasIndex(a => new { a.TenantId, a.OccurredAt });
    }
}
