using LegalPlatform.Infrastructure.Persistence;
using LegalPlatform.Modules.Audit;
using LegalPlatform.Modules.Tenancy;
using LegalPlatform.SharedKernel.Multitenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace LegalPlatform.Api.Persistence;

/// <summary>
/// Design-time factory so <c>dotnet ef migrations</c> can build <see cref="AppDbContext"/> without the
/// full DI container. Uses a no-tenant context and the module assemblies that own EF configurations.
/// </summary>
public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        // Prefer the configured connection string (env/CI) so migrations run against the real DB;
        // fall back to a password-less localhost default for offline model operations.
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Postgres");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            connectionString = "Host=localhost;Port=5432;Database=legalplatform;Username=postgres";
        }

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        var registry = new ModuleRegistry(new[]
        {
            typeof(TenancyModule).Assembly,
            typeof(AuditModule).Assembly,
        });

        return new AppDbContext(options, new DesignTimeTenantContext(), registry);
    }

    private sealed class DesignTimeTenantContext : ITenantContext
    {
        public Guid TenantId => Guid.Empty;
        public bool HasTenant => false;
    }
}
