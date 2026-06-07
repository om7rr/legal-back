using LegalPlatform.Modules.Identity.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LegalPlatform.Modules.Identity.Persistence;

/// <summary>EF mapping for <see cref="AppUser"/>. Owned by the Identity context.</summary>
public sealed class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        builder.ToTable("users", "identity");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.NationalId).IsRequired().HasMaxLength(20);
        builder.HasIndex(u => u.NationalId).IsUnique();
        builder.Property(u => u.FullName).IsRequired().HasMaxLength(200);
        builder.Property(u => u.TenantId).IsRequired();
        builder.Property(u => u.Role).IsRequired().HasConversion<string>().HasMaxLength(30);
        builder.Property(u => u.CreatedAt).IsRequired();
    }
}
