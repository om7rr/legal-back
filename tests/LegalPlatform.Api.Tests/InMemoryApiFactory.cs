using LegalPlatform.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LegalPlatform.Api.Tests;

/// <summary>Shares one <see cref="InMemoryApiFactory"/> (a single host) across all API test classes.</summary>
[CollectionDefinition(Name)]
public sealed class ApiCollection : ICollectionFixture<InMemoryApiFactory>
{
    public const string Name = "api";
}

/// <summary>
/// Hosts the real API but swaps the Npgsql DbContext for EF InMemory, so endpoint tests run anywhere
/// (no Postgres). Tenant isolation is still exercised through the real middleware pipeline.
/// </summary>
public sealed class InMemoryApiFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = $"api-tests-{Guid.NewGuid():N}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureServices(services =>
        {
            var toRemove = services
                .Where(d =>
                    d.ServiceType == typeof(DbContextOptions<AppDbContext>) ||
                    d.ServiceType == typeof(DbContextOptions) ||
                    d.ServiceType == typeof(AppDbContext) ||
                    // The provider (Npgsql) is registered via IDbContextOptionsConfiguration<AppDbContext>;
                    // match by type name (namespace-agnostic) so InMemory is the only provider left.
                    d.ServiceType.Name.StartsWith("IDbContextOptionsConfiguration", StringComparison.Ordinal))
                .ToList();
            foreach (var descriptor in toRemove)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(_dbName));
        });
    }
}
