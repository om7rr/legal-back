using LegalPlatform.Modules.Cases.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LegalPlatform.Modules.Cases.Persistence;

/// <summary>EF mapping for <see cref="Case"/>. Owned by the Cases context.</summary>
public sealed class CaseConfiguration : IEntityTypeConfiguration<Case>
{
    public void Configure(EntityTypeBuilder<Case> builder)
    {
        builder.ToTable("cases", "cases");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.TenantId).IsRequired();
        builder.Property(c => c.CaseNumber).IsRequired().HasMaxLength(50);
        builder.Property(c => c.Title).IsRequired().HasMaxLength(300);
        builder.Property(c => c.Type).HasMaxLength(100);
        builder.Property(c => c.Court).HasMaxLength(200);
        builder.Property(c => c.Status).IsRequired().HasMaxLength(30);
        builder.Property(c => c.CreatedAt).IsRequired();
        // Case number unique within a tenant.
        builder.HasIndex(c => new { c.TenantId, c.CaseNumber }).IsUnique();
    }
}
