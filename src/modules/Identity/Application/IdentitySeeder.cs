using LegalPlatform.Modules.Identity.Domain;
using LegalPlatform.SharedKernel.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LegalPlatform.Modules.Identity.Application;

/// <summary>
/// Seeds the test environment with known firms (tenants) and users so the mock Nafath flow can
/// authenticate real identities (ADR-0006). Idempotent. Test-environment data only.
/// </summary>
public static class IdentitySeeder
{
    public static readonly Guid FirmA = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000001");
    public static readonly Guid FirmB = Guid.Parse("bbbbbbbb-0000-0000-0000-000000000002");

    public static async Task SeedAsync(IAppDbContext db, CancellationToken cancellationToken = default)
    {
        await EnsureUserAsync(db, "1111111111", "مدير المكتب (أ)", FirmA, UserRole.FirmAdmin, cancellationToken);
        await EnsureUserAsync(db, "2222222222", "محامٍ (أ)", FirmA, UserRole.Lawyer, cancellationToken);
        await EnsureUserAsync(db, "3333333333", "مدير المكتب (ب)", FirmB, UserRole.FirmAdmin, cancellationToken);
    }

    private static async Task EnsureUserAsync(
        IAppDbContext db,
        string nationalId,
        string fullName,
        Guid tenantId,
        UserRole role,
        CancellationToken cancellationToken)
    {
        var exists = await db.Set<AppUser>().AnyAsync(u => u.NationalId == nationalId, cancellationToken);
        if (!exists)
        {
            db.Set<AppUser>().Add(new AppUser(nationalId, fullName, tenantId, role));
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
