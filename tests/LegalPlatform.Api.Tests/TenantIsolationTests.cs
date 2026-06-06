using LegalPlatform.Infrastructure.Persistence;
using LegalPlatform.Modules.Audit;
using LegalPlatform.Modules.Audit.Domain;
using LegalPlatform.SharedKernel.Multitenancy;
using Microsoft.EntityFrameworkCore;

namespace LegalPlatform.Api.Tests;

/// <summary>
/// Proves the persistence layer's global query filter isolates tenants: tenant A can never read
/// tenant B's rows. Uses the EF InMemory provider so it runs anywhere (no Postgres required).
/// </summary>
public sealed class TenantIsolationTests
{
    private static readonly Guid TenantA = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid TenantB = Guid.Parse("22222222-2222-2222-2222-222222222222");

    private sealed class FakeTenantContext : ITenantContext
    {
        public Guid TenantId { get; set; }
        public bool HasTenant => TenantId != Guid.Empty;
    }

    private static AppDbContext NewContext(string dbName, Guid tenantId)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        var registry = new ModuleRegistry(new[] { typeof(AuditModule).Assembly });
        return new AppDbContext(options, new FakeTenantContext { TenantId = tenantId }, registry);
    }

    [Fact]
    public async Task Tenant_A_cannot_read_tenant_B_rows()
    {
        var dbName = $"tenant-iso-{Guid.NewGuid()}";

        // Seed one row for each tenant (writes are not filtered).
        await using (var seed = NewContext(dbName, TenantA))
        {
            seed.Set<AuditEntry>().Add(new AuditEntry(TenantA, "case.filed", "Case", "A-1", null, "", "hashA"));
            seed.Set<AuditEntry>().Add(new AuditEntry(TenantB, "case.filed", "Case", "B-1", null, "", "hashB"));
            await seed.SaveChangesAsync();
        }

        // Read as tenant A — the global query filter must hide tenant B's row.
        await using var asTenantA = NewContext(dbName, TenantA);
        var visible = await asTenantA.Set<AuditEntry>().ToListAsync();

        Assert.Single(visible);
        Assert.Equal(TenantA, visible[0].TenantId);
        Assert.Equal("A-1", visible[0].EntityId);
    }

    [Fact]
    public async Task Ignoring_query_filters_reveals_all_rows()
    {
        var dbName = $"tenant-iso-{Guid.NewGuid()}";

        await using (var seed = NewContext(dbName, TenantA))
        {
            seed.Set<AuditEntry>().Add(new AuditEntry(TenantA, "case.filed", "Case", "A-1", null, "", "hashA"));
            seed.Set<AuditEntry>().Add(new AuditEntry(TenantB, "case.filed", "Case", "B-1", null, "", "hashB"));
            await seed.SaveChangesAsync();
        }

        await using var ctx = NewContext(dbName, TenantA);
        var all = await ctx.Set<AuditEntry>().IgnoreQueryFilters().ToListAsync();

        Assert.Equal(2, all.Count);
    }
}
