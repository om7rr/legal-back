using System.Reflection;
using LegalPlatform.Infrastructure.Events;
using LegalPlatform.Infrastructure.Persistence;
using LegalPlatform.SharedKernel.Events;
using LegalPlatform.SharedKernel.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace LegalPlatform.Infrastructure;

/// <summary>Registers the shared technical services (persistence, cache) for the platform.</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        params Assembly[] moduleAssemblies)
    {
        services.AddSingleton(new ModuleRegistry(moduleAssemblies));

        var postgres = configuration.GetConnectionString("Postgres");
        if (string.IsNullOrWhiteSpace(postgres))
        {
            // Dev fallback: localhost, NO password committed to source. /health/ready reports
            // unhealthy until a real connection string is supplied via env or user-secrets.
            postgres = "Host=localhost;Port=5432;Database=legalplatform;Username=postgres";
        }

        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(postgres));
        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<IEventBus, InProcessEventBus>();

        var redis = configuration.GetConnectionString("Redis");
        if (string.IsNullOrWhiteSpace(redis))
        {
            redis = "localhost:6379";
        }

        services.AddSingleton<IConnectionMultiplexer>(_ =>
        {
            var options = ConfigurationOptions.Parse(redis);
            options.AbortOnConnectFail = false; // don't throw at startup when Redis is down
            options.ConnectTimeout = 2000;
            return ConnectionMultiplexer.Connect(options);
        });

        return services;
    }
}
